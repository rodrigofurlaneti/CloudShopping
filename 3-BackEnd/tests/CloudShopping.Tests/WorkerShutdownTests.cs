using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class WorkerShutdownTests
{
    [Theory]
    [InlineData("asaas")]
    [InlineData("reservations")]
    [InlineData("imports")]
    [InlineData("notifications")]
    public async Task Stopping_while_waiting_for_tick_completes_without_cancelled_or_faulted_task(string name)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var config = new ConfigurationBuilder().Build();
        using BackgroundService worker = name switch
        {
            "asaas" => new AsaasWorker(services, config, NullLogger<AsaasWorker>.Instance),
            "reservations" => new ReservationExpiryWorker(services, config, NullLogger<ReservationExpiryWorker>.Instance),
            "imports" => new ImportWorker(services, config, NullLogger<ImportWorker>.Instance),
            _ => new NotificationWorker(services, config, NullLogger<NotificationWorker>.Instance)
        };
        await worker.StartAsync(CancellationToken.None);
        Assert.NotNull(worker.ExecuteTask);
        Assert.False(worker.ExecuteTask.IsCompleted);
        await worker.StopAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5));
        await worker.ExecuteTask.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(worker.ExecuteTask.IsCompletedSuccessfully);
    }
}
