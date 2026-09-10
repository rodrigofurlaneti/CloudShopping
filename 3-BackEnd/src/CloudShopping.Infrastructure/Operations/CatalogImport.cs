using CloudShopping.Domain.Exceptions;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Operations;
public sealed class CatalogImport
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string ContentHash {get;set;}="";
 public string State {get;set;}="Preview";
 public string Actor {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class CatalogImportRow
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string ImportId {get;set;}="";
 public int LineNumber {get;set;}
 public string Sku {get;set;}="";
 public string Name {get;set;}="";
 public int DepartmentId {get;set;}
 public decimal Price {get;set;}
 public int PhysicalStock {get;set;}
 public int? ExpectedProductId {get;set;}
 public int? ExpectedVersion {get;set;}
 public string State {get;set;}="Pending";
 public string Error {get;set;}="";
}
public sealed class CatalogImportService(AppDbContext db)
{
 public async Task<object> List(int page,CancellationToken ct)=>await db.Set<CatalogImport>().OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).ToListAsync(ct);
 public async Task<object> Detail(string id,CancellationToken ct)=>new{job=await db.Set<CatalogImport>().AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException(),rows=await db.Set<CatalogImportRow>().AsNoTracking().Where(x=>x.ImportId==id).OrderBy(x=>x.LineNumber).ToListAsync(ct)};
 public async Task<object> Preview(string csv,string actor,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(csv)||csv.Length>200000)throw new ArgumentException("CSV obrigatório, até 200 mil caracteres e 200 linhas de produto.");
  var hash=JsonFields.Hash(csv);await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"import-preview:"+db.CurrentTenantId+":"+hash,ct);
  var previous=await db.Set<CatalogImport>().SingleOrDefaultAsync(x=>x.ContentHash==hash,ct);if(previous!=null)return await Detail(previous.Id,ct);
  using var parser=new TextFieldParser(new StringReader(csv)){HasFieldsEnclosedInQuotes=true,TrimWhiteSpace=true};parser.SetDelimiters(",");
  if(!(parser.ReadFields()??[]).SequenceEqual(new[]{"sku","name","departmentId","price","physicalStock"}))throw new ArgumentException("Cabeçalho esperado: sku,name,departmentId,price,physicalStock");
  var job=new CatalogImport {TenantId=db.CurrentTenantId,ContentHash=hash,Actor=actor};var rows=new List<CatalogImportRow>();var codes=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  while(!parser.EndOfData)
  {
   if(rows.Count>=200)throw new ArgumentException("Máximo de 200 linhas por lote.");
   string[] fields;try{fields=parser.ReadFields()??[];}catch(MalformedLineException){throw new ArgumentException("CSV malformado. Confira aspas e separadores.");}
   if(fields.Length!=5)throw new ArgumentException("Cada linha precisa de cinco campos.");
   var r=new CatalogImportRow {TenantId=db.CurrentTenantId,ImportId=job.Id,LineNumber=rows.Count+2,Sku=fields[0],Name=fields[1]};
   if(r.Sku.Length is <1 or >30||r.Name.Length is <1 or >150||!codes.Add(r.Sku)||!int.TryParse(fields[2],out var dept)||!decimal.TryParse(fields[3],NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var price)||!int.TryParse(fields[4],out var stock)||price<=0||price>1000000||decimal.Round(price,2)!=price||stock is <0 or >1000000)
    throw new ArgumentException($"Linha {r.LineNumber}: SKU único, nome, departamento, preço com ponto decimal e estoque válidos são obrigatórios.");
   r.DepartmentId=dept;r.Price=price;r.PhysicalStock=stock;
   var p=await db.Products.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.TenantId==db.CurrentTenantId&&x.Sku==r.Sku,ct);
   r.ExpectedProductId=p?.Id;r.ExpectedVersion=p?.Version;
   if(!await db.Departments.AnyAsync(x=>x.Id==dept,ct)){r.State="Invalid";r.Error="Departamento inexistente ou fora da loja.";}
   else if(p!=null&&stock<p.ReservedStock){r.State="Invalid";r.Error="Estoque físico proposto é inferior ao reservado.";}
   rows.Add(r);
  }
  if(rows.Count==0)throw new ArgumentException("CSV sem produtos.");
  db.Add(job);db.AddRange(rows);await db.SaveChangesAsync(ct);return await Detail(job.Id,ct);
 }
 public async Task Queue(string id,CancellationToken ct)
 {
  await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"import:"+id,ct);
  var job=await db.Set<CatalogImport>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();
  if(job.State!="Preview")return;
  if(await db.Set<CatalogImportRow>().AnyAsync(x=>x.ImportId==id&&x.State=="Invalid",ct))throw new CommerceConflictException("Corrija as linhas inválidas e envie novo arquivo.");
  job.State="Queued";await db.SaveChangesAsync(ct);
 }
 public async Task Process(string id,CancellationToken ct)
 {
  await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"import:"+id,ct);db.ChangeTracker.Clear();
  var job=await db.Set<CatalogImport>().SingleAsync(x=>x.Id==id,ct);if(job.State!="Queued")return;
  var ids=await db.Set<CatalogImportRow>().Where(x=>x.ImportId==id&&x.State=="Pending").OrderBy(x=>x.LineNumber).Take(25).Select(x=>x.Id).ToListAsync(ct);
  foreach(var rowId in ids)
  {
   db.ChangeTracker.Clear();var row=await db.Set<CatalogImportRow>().SingleAsync(x=>x.Id==rowId,ct);
   try
   {
    await using var tx=await db.Database.BeginTransactionAsync(ct);
    var p=await db.Products.IgnoreQueryFilters().SingleOrDefaultAsync(x=>x.TenantId==db.CurrentTenantId&&x.Sku==row.Sku,ct);
    if(p?.Id!=row.ExpectedProductId||p?.Version!=row.ExpectedVersion)throw new CommerceConflictException("Produto mudou após a prévia; gere nova importação revisada.");
    if(!await db.Departments.AnyAsync(x=>x.Id==row.DepartmentId,ct))throw new CommerceConflictException("Departamento não está disponível.");
    int oldStock=p?.PhysicalStock??0;
    if(p==null){p=Product.Create(db.CurrentTenantId,row.DepartmentId,row.Sku,row.Name,row.Price,row.PhysicalStock);db.Add(p);await db.SaveChangesAsync(ct);}
    else {p.UpdateDetails(row.Name,row.Price);p.ChangeDepartment(row.DepartmentId);p.AdjustInventory(row.PhysicalStock);}
    var difference=row.PhysicalStock-oldStock;
    if(difference!=0)db.Add(StockMovement.Create(p.Id,StockMovementType.Adjustment,difference,p.PhysicalStock,"Importação "+id+" por "+job.Actor));
    row.State="Completed";await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
   }
   catch(Exception e)when(e is CommerceConflictException or ArgumentException or InvalidOperationException or DbUpdateConcurrencyException)
   {
    db.ChangeTracker.Clear();row=await db.Set<CatalogImportRow>().SingleAsync(x=>x.Id==rowId,ct);row.State="Failed";row.Error="Dados mudaram ou violam as regras de estoque/catálogo. Gere nova prévia com o arquivo corrigido.";await db.SaveChangesAsync(ct);
   }
  }
  db.ChangeTracker.Clear();job=await db.Set<CatalogImport>().SingleAsync(x=>x.Id==id,ct);
  if(!await db.Set<CatalogImportRow>().AnyAsync(x=>x.ImportId==id&&x.State=="Pending",ct))
  {job.State=await db.Set<CatalogImportRow>().AnyAsync(x=>x.ImportId==id&&x.State!="Completed",ct)?"CompletedWithErrors":"Completed";await db.SaveChangesAsync(ct);}
 }
}
