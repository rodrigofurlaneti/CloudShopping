using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Operations;
public sealed class ImportWorker(IServiceProvider services,IConfiguration config,ILogger<ImportWorker> log):BackgroundService
{
 protected override async Task ExecuteAsync(CancellationToken ct)
 {
  using var timer=new PeriodicTimer(TimeSpan.FromSeconds(15));
  try{while(await timer.WaitForNextTickAsync(ct))try{await RunOnce(ct);}catch(Exception e)when(e is not OperationCanceledException){log.LogWarning("Importações pendentes serão retomadas no próximo ciclo.");}}
  catch(OperationCanceledException)when(ct.IsCancellationRequested){}
 }
 public async Task RunOnce(CancellationToken ct)
 {
  await using var c=new MySqlConnection(config.GetConnectionString("DefaultConnection"));await c.OpenAsync(ct);
  using var command=new MySqlCommand("SELECT Id,TenantId FROM catalogimports WHERE State='Queued' ORDER BY CreatedAt LIMIT 5",c);
  var rows=new List<(string,int)>();await using(var r=await command.ExecuteReaderAsync(ct))while(await r.ReadAsync(ct))rows.Add((r.GetString(0),r.GetInt32(1)));
  foreach(var (id,tenant) in rows)
  {using var scope=services.CreateScope();await using var db=new AppDbContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>(),new WorkerTenant(tenant));await new CatalogImportService(db).Process(id,ct);}
 }
 private sealed record WorkerTenant(int Id):ITenantProvider{public int GetTenantId()=>Id;}
}
