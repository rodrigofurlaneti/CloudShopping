using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Operations;

public sealed record ReviewInput(int OrderId,int Rating,string Content);
public sealed record ReviewDecisionInput(int Version,string State,string Reason);
public sealed record TicketInput(string Key,string Subject,string Category,int? OrderId,string Content);
public sealed record MessageInput(string Key,int Version,string Content,bool Close=false);
public sealed class CustomerEngagement(AppDbContext db)
{
 private static void Validate(string? s,int max,string name){if(string.IsNullOrWhiteSpace(s)||s.Length>max)throw new ArgumentException($"{name}: tamanho permitido de 1 a {max} caracteres.");}
 private static void Page(int page){if(page is <1 or >100000)throw new ArgumentException("Página inválida.");}
 public async Task<object> Favorites(int customer,int page,CancellationToken ct)
 {
  Page(page);return await (from w in db.Set<WishlistItem>() join p in db.Products on w.ProductId equals p.Id where w.CustomerId==customer orderby w.CreatedAt descending
   select new{p.Id,p.Name,p.Sku,p.Price,available=p.PhysicalStock-p.ReservedStock}).Skip((page-1)*20).Take(20).ToListAsync(ct);
 }
 public async Task Favorite(int customer,int product,bool enabled,CancellationToken ct)
 {
  await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"favorite:"+db.CurrentTenantId+":"+customer+":"+product,ct);
  var item=await db.Set<WishlistItem>().SingleOrDefaultAsync(x=>x.CustomerId==customer&&x.ProductId==product,ct);
  if(enabled&&item==null)
  {if(!await db.Products.AnyAsync(x=>x.Id==product,ct))throw new KeyNotFoundException("Produto indisponível.");db.Add(new WishlistItem {TenantId=db.CurrentTenantId,CustomerId=customer,ProductId=product});}
  else if(!enabled&&item!=null)db.Remove(item);
  await db.SaveChangesAsync(ct);
 }
 public async Task<object> Reviews(int product,int page,CancellationToken ct)
 {
  Page(page);if(!await db.Products.AnyAsync(x=>x.Id==product,ct))throw new KeyNotFoundException();
  var q=db.Set<ProductReview>().AsNoTracking().Where(x=>x.ProductId==product&&x.State=="Approved");
  return new {count=await q.CountAsync(ct),average=await q.Select(x=>(double?)x.Rating).AverageAsync(ct),items=await q.OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).Select(x=>new{x.Id,x.Rating,x.Content,x.CreatedAt,verifiedPurchase=true}).ToListAsync(ct)};
 }
 public async Task<object> Review(int customer,int product,ReviewInput input,CancellationToken ct)
 {
  Validate(input.Content,2000,"Avaliação");if(input.Rating is <1 or >5)throw new ArgumentException("Nota entre 1 e 5.");
  await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"review:"+db.CurrentTenantId+":"+customer+":"+product,ct);
  var existing=await db.Set<ProductReview>().SingleOrDefaultAsync(x=>x.CustomerId==customer&&x.ProductId==product,ct);
  if(existing!=null)
  {if(existing.OrderId==input.OrderId&&existing.Rating==input.Rating&&existing.Content==input.Content)return new{existing.Id,existing.State};throw new CommerceConflictException("Você já avaliou este produto.");}
  var eligible=await db.Orders.AnyAsync(x=>x.Id==input.OrderId&&x.CustomerId==customer&&x.FulfillmentState=="Delivered"&&x.OrderItems.Any(i=>i.ProductId==product),ct);
  if(!eligible)throw new CommerceConflictException("Informe uma compra sua, entregue, que contenha o produto.");
  var review=new ProductReview {TenantId=db.CurrentTenantId,CustomerId=customer,ProductId=product,OrderId=input.OrderId,Rating=input.Rating,Content=input.Content};
  db.Add(review);await db.SaveChangesAsync(ct);return new{review.Id,review.State};
 }
 public async Task<object> Moderation(int page,string state,CancellationToken ct)
 {Page(page);return await db.Set<ProductReview>().AsNoTracking().Where(x=>x.State==state).OrderBy(x=>x.CreatedAt).Skip((page-1)*20).Take(20).ToListAsync(ct);}
 public async Task Moderate(string id,ReviewDecisionInput input,string actor,CancellationToken ct)
 {
  Validate(input.Reason,1000,"Motivo da moderação");if(input.State is not("Approved" or "Rejected"))throw new ArgumentException("Decisão inválida.");
  var r=await db.Set<ProductReview>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();
  if(r.State==input.State&&r.ModerationReason==input.Reason)return;
  if(r.Version!=input.Version)throw new CommerceConflictException("Avaliação alterada por outro operador.");
  r.State=input.State;r.ModerationReason=input.Reason;
  db.Add(new ReviewDecision {TenantId=db.CurrentTenantId,ReviewId=id,State=input.State,Reason=input.Reason,Actor=actor});
  await db.SaveChangesAsync(ct);
 }
 public async Task<object> Tickets(int? customer,int page,CancellationToken ct)
 {Page(page);return await db.Set<SupportTicket>().AsNoTracking().Where(x=>customer==null||x.CustomerId==customer).OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).ToListAsync(ct);}
 public async Task<object> Ticket(string id,int? customer,CancellationToken ct)
 {
  var ticket=await db.Set<SupportTicket>().AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&(customer==null||x.CustomerId==customer),ct)??throw new KeyNotFoundException("Protocolo não encontrado.");
  return new{ticket,messages=await db.Set<SupportMessage>().AsNoTracking().Where(x=>x.TicketId==id).OrderBy(x=>x.CreatedAt).ToListAsync(ct)};
 }
 public async Task<object> OpenTicket(int customer,TicketInput input,CancellationToken ct)
 {
  if(!Guid.TryParse(input.Key,out var key))throw new ArgumentException("Chave inválida.");Validate(input.Subject,150,"Assunto");Validate(input.Content,4000,"Mensagem");
  if(input.Category is not("Question" or "Order" or "Privacy"))throw new ArgumentException("Categoria inválida.");
  var id=key.ToString("N");await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"ticket:"+id,ct);
  var existing=await db.Set<SupportTicket>().SingleOrDefaultAsync(x=>x.Id==id&&x.CustomerId==customer,ct);
  if(existing!=null)
  {
   var first=await db.Set<SupportMessage>().SingleAsync(x=>x.TicketId==id&&x.OperationKey=="initial",ct);
   if(existing.Subject!=input.Subject||existing.OrderId!=input.OrderId||existing.Category!=input.Category||first.Content!=input.Content)throw new CommerceConflictException("Chave usada em outra solicitação.");
   return await Ticket(id,customer,ct);
  }
  if(input.OrderId.HasValue&&!await db.Orders.AnyAsync(x=>x.Id==input.OrderId&&x.CustomerId==customer,ct))throw new KeyNotFoundException("Pedido não encontrado.");
  db.Add(new SupportTicket {Id=id,TenantId=db.CurrentTenantId,CustomerId=customer,OrderId=input.OrderId,Subject=input.Subject,Category=input.Category});
  db.Add(new SupportMessage {TenantId=db.CurrentTenantId,TicketId=id,Content=input.Content,Sender="Customer",OperationKey="initial"});
  await db.SaveChangesAsync(ct);return await Ticket(id,customer,ct);
 }
 public async Task<object> Reply(string id,int? customer,MessageInput input,CancellationToken ct)
 {
  Validate(input.Key,100,"Chave");Validate(input.Content,4000,"Mensagem");
  await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"ticket:"+id,ct);
  var ticket=await db.Set<SupportTicket>().SingleOrDefaultAsync(x=>x.Id==id&&(customer==null||x.CustomerId==customer),ct)??throw new KeyNotFoundException();
  var sender=customer==null?"Support":"Customer";
  var existing=await db.Set<SupportMessage>().SingleOrDefaultAsync(x=>x.TicketId==id&&x.OperationKey==input.Key,ct);
  if(existing!=null){if(existing.Content!=input.Content||existing.Sender!=sender)throw new CommerceConflictException("Chave já utilizada.");return await Ticket(id,customer,ct);}
  if(ticket.Version!=input.Version)throw new CommerceConflictException("O protocolo recebeu uma atualização. Recarregue.");
  if(customer!=null&&input.Close)throw new UnauthorizedAccessException();
  ticket.State=input.Close?"Closed":"Open";db.Entry(ticket).Property(x=>x.Version).IsModified=true;
  db.Add(new SupportMessage {TenantId=db.CurrentTenantId,TicketId=id,Content=input.Content,Sender=sender,OperationKey=input.Key});
  await db.SaveChangesAsync(ct);return await Ticket(id,customer,ct);
 }
}
