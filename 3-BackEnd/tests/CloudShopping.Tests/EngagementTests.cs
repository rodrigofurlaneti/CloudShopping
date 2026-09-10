using CloudShopping.Domain.Exceptions;
using System.Text.Json;
using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Services;
using CloudShopping.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using Xunit;
public sealed partial class CommerceTests
{
 [Fact]
 public async Task Favorites_are_idempotent_and_scoped_to_customer_and_tenant()
 {
  await using var db=Db(1);var service=new CustomerEngagement(db);
  await service.Favorite(customerId,productId,true,default);await service.Favorite(customerId,productId,true,default);
  Assert.Single(await db.Set<WishlistItem>().ToListAsync());
  await using var other=Db(2);Assert.Empty(await other.Set<WishlistItem>().ToListAsync());
  await service.Favorite(customerId,productId,false,default);Assert.Empty(await db.Set<WishlistItem>().ToListAsync());
 }
 [Fact]
 public async Task Reviews_require_delivered_purchase_and_moderation_changes_public_average()
 {
  await using var db=Db(1);var id=await PackedOrder(db);var service=new CustomerEngagement(db);
  await Assert.ThrowsAsync<CommerceConflictException>(()=>service.Review(customerId,productId,new(id,1,"Experiência ruim"),default));
  var o=await db.Orders.SingleAsync();var i=await db.OrderItems.SingleAsync();
  await new OrderOperations(db).Dispatch(id,new("ship",o.Version,"Carrier","Normal","TRACK",1,[new(i.Id,1)]),"Administrator:1",default);
  o=await db.Orders.SingleAsync();var s=await db.Set<Shipment>().SingleAsync();
  await new OrderOperations(db).Track(id,s.Id,new("delivery",o.Version,"Delivered","Comprovante conferido"),"Administrator:1",default);
  await service.Review(customerId,productId,new(id,1,"<script>alert(1)</script>"),default);
  var before=JsonSerializer.SerializeToElement(await service.Reviews(productId,1,default));Assert.Equal(0,before.GetProperty("count").GetInt32());
  var r=await db.Set<ProductReview>().SingleAsync();await service.Moderate(r.Id,new(r.Version,"Approved","Relato elegível"),"Administrator:1",default);
  var after=JsonSerializer.SerializeToElement(await service.Reviews(productId,1,default));Assert.Equal(1,after.Money("average"));
  Assert.Single(await db.Set<ReviewDecision>().ToListAsync());
  await using var other=Db(2);await Assert.ThrowsAsync<KeyNotFoundException>(()=>new CustomerEngagement(other).Moderate(r.Id,new(r.Version,"Rejected","Outra loja"),"Administrator:2",default));
 }
 [Fact]
 public async Task Support_replay_and_owner_checks_preserve_one_protocol_and_message()
 {
  await using var db=Db(1);var service=new CustomerEngagement(db);var key=Guid.NewGuid().ToString();var input=new TicketInput(key,"Preciso de ajuda","Privacy",null,"Solicito informações sobre meus dados");
  await service.OpenTicket(customerId,input,default);await service.OpenTicket(customerId,input,default);var t=await db.Set<SupportTicket>().SingleAsync();
  Assert.Single(await db.Set<SupportMessage>().ToListAsync());
  await Assert.ThrowsAsync<KeyNotFoundException>(()=>service.Ticket(t.Id,customerId+10,default));
  var reply=new MessageInput("reply",t.Version,"Recebido para análise",true);
  await service.Reply(t.Id,null,reply,default);await service.Reply(t.Id,null,reply,default);
  Assert.Equal(2,await db.Set<SupportMessage>().CountAsync());Assert.Equal("Closed",(await db.Set<SupportTicket>().SingleAsync()).State);
  await using var other=Db(2);Assert.Empty(await other.Set<SupportTicket>().ToListAsync());
 }
 [Fact]
 public async Task Reports_use_real_payment_cohort_and_do_not_mix_other_tenants()
 {
  await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
  await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";await Payments(db,gateway).Reconcile(id,customerId,default);
  var today=DateOnly.FromDateTime(DateTime.UtcNow);var report=JsonSerializer.SerializeToElement(await new CloudShopping.Application.Features.Reports.Queries.GetReportSummary.GetReportSummaryQueryHandler(new CloudShopping.Infrastructure.Repositories.ReportRepository(db)).Handle(new(today,today),default));
  Assert.Equal(115m,report.Money("ApprovedGross"));Assert.Equal(1,report.GetProperty("OrderCount").GetInt32());
  await using var other=Db(2);var otherReport=JsonSerializer.SerializeToElement(await new CloudShopping.Application.Features.Reports.Queries.GetReportSummary.GetReportSummaryQueryHandler(new CloudShopping.Infrastructure.Repositories.ReportRepository(other)).Handle(new(today,today),default));Assert.Equal(0,otherReport.GetProperty("OrderCount").GetInt32());
  Assert.StartsWith("\"'",CloudShopping.Application.Features.Reports.ReportCsv.Cell(" =HYPERLINK(\"test\")"));
 }
}
