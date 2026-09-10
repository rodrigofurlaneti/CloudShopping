using CloudShopping.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore;
using Xunit;
public sealed partial class CommerceTests
{
 [Fact]
 public async Task Import_preview_is_readonly_and_repeat_execution_does_not_duplicate_stock()
 {
  await using var db=Db(1);var dept=(await db.Products.SingleAsync()).DepartmentId;var csv=$"sku,name,departmentId,price,physicalStock\nSKU-NEW,Produto importado,{dept},19.90,5";
  var service=new CatalogImportService(db);await service.Preview(csv,"Administrator:1",default);await service.Preview(csv,"Administrator:1",default);
  Assert.Single(await db.Products.ToListAsync());var job=await db.Set<CatalogImport>().SingleAsync();
  await service.Queue(job.Id,default);await service.Process(job.Id,default);await service.Process(job.Id,default);
  Assert.Equal(2,await db.Products.CountAsync());Assert.Single(await db.StockMovements.ToListAsync());Assert.Equal("Completed",(await db.Set<CatalogImport>().SingleAsync()).State);
  await using var other=Db(2);await Assert.ThrowsAsync<KeyNotFoundException>(()=>new CatalogImportService(other).Detail(job.Id,default));
 }
 [Fact]
 public async Task Import_rejects_stale_inventory_instead_of_overwriting_new_reservations()
 {
  await using var db=Db(1);var p=await db.Products.SingleAsync();var csv=$"sku,name,departmentId,price,physicalStock\n{p.Sku},Alterado,{p.DepartmentId},20.00,0";
  var service=new CatalogImportService(db);await service.Preview(csv,"Administrator:1",default);var job=await db.Set<CatalogImport>().SingleAsync();await service.Queue(job.Id,default);
  p.ReserveStock(1);await db.SaveChangesAsync();await service.Process(job.Id,default);
  Assert.Equal("CompletedWithErrors",(await db.Set<CatalogImport>().SingleAsync()).State);var current=await db.Products.SingleAsync();Assert.Equal(1,current.ReservedStock);Assert.Equal(100,current.Price);
 }
 [Fact]
 public async Task Invalid_department_remains_in_preview_but_cannot_be_queued()
 {
  await using var db=Db(1);var service=new CatalogImportService(db);await service.Preview("sku,name,departmentId,price,physicalStock\nNEW,Invalid department,9999,20.00,1","Administrator:1",default);
  var job=await db.Set<CatalogImport>().SingleAsync();Assert.Equal("Invalid",(await db.Set<CatalogImportRow>().SingleAsync()).State);
  await Assert.ThrowsAsync<CloudShopping.Infrastructure.Services.CommerceConflictException>(()=>service.Queue(job.Id,default));Assert.Single(await db.Products.ToListAsync());
 }
}
