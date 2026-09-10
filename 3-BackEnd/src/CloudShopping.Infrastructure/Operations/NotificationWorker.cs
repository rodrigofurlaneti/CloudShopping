using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Operations;

// Local portal consumer. Email delivery is a separate consumer and is never claimed here.
public sealed class NotificationWorker(IServiceProvider services,IConfiguration config,ILogger<NotificationWorker> log):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken ct)
 {
  using var timer=new PeriodicTimer(TimeSpan.FromSeconds(15));
  try{while(await timer.WaitForNextTickAsync(ct))try{await RunOnce(ct);}catch(Exception e)when(e is not OperationCanceledException){log.LogWarning("Fila de notificações será retomada no próximo ciclo.");}}
  catch(OperationCanceledException)when(ct.IsCancellationRequested){}
 }
 public async Task RunOnce(CancellationToken ct)
 {
  var connection=config.GetConnectionString("DefaultConnection")!;
  await using var c=new MySqlConnection(connection);await c.OpenAsync(ct);
  using var select=new MySqlCommand("SELECT Id,TenantId FROM commerceoutbox WHERE State='Pending' AND AvailableAt<=UTC_TIMESTAMP(6) ORDER BY CreatedAt LIMIT 50",c);
  var rows=new List<(string Id,int Tenant)>();await using(var r=await select.ExecuteReaderAsync(ct))while(await r.ReadAsync(ct))rows.Add((r.GetString(0),r.GetInt32(1)));
  foreach(var row in rows)
  {
   await using var lease=await PaymentLock.Acquire(connection,"notification:"+row.Id,ct);
   using var scope=services.CreateScope();
   await using var db=new AppDbContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>(),new WorkerTenant(row.Tenant));
   try
   {
    await using var tx=await db.Database.BeginTransactionAsync(ct);
    var message=await db.Set<CommerceOutbox>().SingleAsync(x=>x.Id==row.Id,ct);
    if(message.State!="Pending")continue;
    var order=await db.Orders.SingleAsync(x=>x.Id==message.OrderId,ct);
    if(!await db.Set<CustomerNotification>().AnyAsync(x=>x.EventId==message.Id,ct))
     db.Add(new CustomerNotification {TenantId=row.Tenant,CustomerId=order.CustomerId,OrderId=order.Id,EventId=message.Id,Kind=message.Kind});
    message.State="Processed";message.ProcessedAt=DateTime.UtcNow;message.LastError=null;
    await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
   }
   catch(Exception e)when(e is not OperationCanceledException)
   {
    using var retry=new MySqlCommand("UPDATE commerceoutbox SET Attempts=Attempts+1,State=IF(Attempts>=8,'Failed','Pending'),AvailableAt=UTC_TIMESTAMP(6)+INTERVAL 5 MINUTE,LastError='Entrega no portal pendente; verificar recurso e consumidor' WHERE Id=@id AND State='Pending'",c);
    retry.Parameters.AddWithValue("@id",row.Id);await retry.ExecuteNonQueryAsync(ct);
   }
  }
 }
 private sealed record WorkerTenant(int Id):ITenantProvider{public int GetTenantId()=>Id;}
}
