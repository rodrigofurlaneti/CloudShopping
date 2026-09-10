using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Services;

public sealed class ReservationExpiryWorker(IServiceProvider services, IConfiguration config, ILogger<ReservationExpiryWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                try
                {
                    await RunOnce(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
                catch (Exception ex) { log.LogError(ex, "Falha ao expirar reservas; será repetido no próximo ciclo."); }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
    public async Task RunOnce(CancellationToken ct)
    {
        await using var connection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync(ct);
        await using var cmd = new MySqlCommand("SELECT DISTINCT TenantId FROM orders WHERE ReservationState='Reserved' AND OrderStatusId=1 AND ReservationExpiresAt <= UTC_TIMESTAMP(6)", connection);
        var tenants = new List<int>();
        await using (var reader = await cmd.ExecuteReaderAsync(ct)) while (await reader.ReadAsync(ct)) tenants.Add(reader.GetInt32(0));
        foreach (var tenant in tenants)
        {
            using var scope = services.CreateScope();
            await using var db = new AppDbContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<AppDbContext>>(), new FixedTenant(tenant));
            var ids = await db.Orders.Where(x => x.ReservationState == "Reserved" && x.OrderStatusId == 1 && x.ReservationExpiresAt <= DateTime.UtcNow &&
                !db.Set<CloudShopping.Infrastructure.Payments.PaymentAttempt>().Any(p=>p.OrderId==x.Id))
                .OrderBy(x => x.Id).Select(x => x.Id).Take(100).ToListAsync(ct);
            var commerce = new StoreCommerceService(db, services.GetRequiredService<IDataProtectionProvider>(), config);
            foreach (var id in ids)
            {
                try { await commerce.Release(id, null, ct); }
                catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); }
            }
        }
    }
    private sealed record FixedTenant(int Id) : ITenantProvider { public int GetTenantId() => Id; }
}

