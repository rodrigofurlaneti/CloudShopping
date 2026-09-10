using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
public sealed partial class CommerceTests
{
 [Fact]
 public async Task Outbox_commit_restart_and_concurrent_consumers_produce_one_notification()
 {
  await using var db=Db(1);var preview=await Prepare(db);await Service(db).Confirm(customerId,Guid.NewGuid().ToString(),preview.Token,default);
  Assert.Single(await db.Set<CommerceOutbox>().ToListAsync());
  var services=new ServiceCollection().AddSingleton(new DbContextOptionsBuilder<AppDbContext>().UseMySql(connection,new MySqlServerVersion(new Version(8,0,43))).Options).BuildServiceProvider();
  var first=new NotificationWorker(services,PaymentConfig,NullLogger<NotificationWorker>.Instance);
  var second=new NotificationWorker(services,PaymentConfig,NullLogger<NotificationWorker>.Instance);
  await Task.WhenAll(first.RunOnce(default),second.RunOnce(default));await second.RunOnce(default);
  db.ChangeTracker.Clear();Assert.Single(await db.Set<CustomerNotification>().ToListAsync());Assert.Equal("Processed",(await db.Set<CommerceOutbox>().SingleAsync()).State);
  await using var other=Db(2);Assert.Empty(await other.Set<CustomerNotification>().ToListAsync());
 }
 [Fact]
 public async Task Outbox_rolls_back_with_order_change()
 {
  await using var db=Db(1);var preview=await Prepare(db);var id=(await Service(db).Confirm(customerId,Guid.NewGuid().ToString(),preview.Token,default)).Id;
  await using(var tx=await db.Database.BeginTransactionAsync())
  {
   var o=await db.Orders.SingleAsync(x=>x.Id==id);o.HoldFinancialReview("Teste de rollback");await db.SaveChangesAsync();await tx.RollbackAsync();
  }
  db.ChangeTracker.Clear();Assert.Single(await db.Set<CommerceOutbox>().ToListAsync());Assert.Equal("Unpaid",(await db.Orders.SingleAsync()).FinancialState);
 }
}
