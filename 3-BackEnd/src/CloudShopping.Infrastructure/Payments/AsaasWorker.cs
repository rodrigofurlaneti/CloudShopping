using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Text.Json;
namespace CloudShopping.Infrastructure.Payments;

public sealed class AsaasWorker(IServiceProvider services,IConfiguration config,ILogger<AsaasWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(15));
        while(await timer.WaitForNextTickAsync(ct))
            try {await RunOnce(ct);}catch(OperationCanceledException)when(ct.IsCancellationRequested){return;}
            catch(Exception){log.LogWarning("Conciliação Asaas será repetida no próximo ciclo.");}
    }
    public async Task RunOnce(CancellationToken ct)
    {
        using var scope=services.CreateScope();var inbox=scope.ServiceProvider.GetRequiredService<AsaasInbox>();
        await using var c=new MySqlConnection(inbox.ConnectionString);await c.OpenAsync(ct);
        using var events=new MySqlCommand("SELECT Id FROM asaasinbox WHERE ProcessedAt IS NULL AND NextCheckAt<=UTC_TIMESTAMP(6) ORDER BY Id LIMIT 50",c);
        var ids=new List<long>();await using(var r=await events.ExecuteReaderAsync(ct))while(await r.ReadAsync(ct))ids.Add(r.GetInt64(0));
        foreach(var id in ids)
        {
            await using var lease=await PaymentLock.Acquire(inbox.ConnectionString,"inbox:"+id,ct);
            try {await ProcessEvent(scope.ServiceProvider,inbox,c,id,ct);}
            catch(Exception ex) when(ex is not OperationCanceledException)
            {using var retry=new MySqlCommand("UPDATE asaasinbox SET Attempts=Attempts+1,NextCheckAt=UTC_TIMESTAMP(6)+INTERVAL 2 MINUTE,LastError='Evento aguardando vínculo ou conciliação' WHERE Id=@id",c);retry.Parameters.AddWithValue("@id",id);await retry.ExecuteNonQueryAsync(ct);}
        }
        using var attempts=new MySqlCommand("SELECT TenantId,OrderId FROM paymentattempts WHERE State<>'Refunded' AND NextCheckAt<=UTC_TIMESTAMP(6) ORDER BY NextCheckAt LIMIT 50",c);
        var pending=new List<(int,int)>();await using(var r=await attempts.ExecuteReaderAsync(ct))while(await r.ReadAsync(ct))pending.Add((r.GetInt32(0),r.GetInt32(1)));
        foreach(var (tenant,order) in pending)
        {
            await using var db=Context(scope.ServiceProvider,tenant);var payment=Service(scope.ServiceProvider,db);
            try {await payment.Reconcile(order,null,ct);}catch(Exception ex)when(ex is not OperationCanceledException){log.LogWarning("Tentativa financeira do pedido {OrderId} será conciliada novamente.",order);}
        }
    }
    private static AppDbContext Context(IServiceProvider sp,int tenant)=>new(sp.GetRequiredService<DbContextOptions<AppDbContext>>(),new WorkerTenant(tenant));
    private AsaasPayments Service(IServiceProvider sp,AppDbContext db)
    {
        var gateway=sp.GetRequiredService<IAsaasGateway>();var protection=sp.GetRequiredService<IDataProtectionProvider>();
        return new(db,gateway,new(db,gateway,protection,config),new(db,protection,config),config);
    }
    private async Task ProcessEvent(IServiceProvider sp,AsaasInbox inbox,MySqlConnection c,long id,CancellationToken ct)
    {
        string account,payload,type;
        using(var read=new MySqlCommand("SELECT AccountKey,ProtectedPayload,EventType FROM asaasinbox WHERE Id=@id AND ProcessedAt IS NULL",c))
        {
            read.Parameters.AddWithValue("@id",id);await using var r=await read.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))return;
            account=r.GetString(0);payload=inbox.Decode(r.GetString(1));type=r.GetString(2);
        }
        using var doc=JsonDocument.Parse(payload);var root=doc.RootElement;
        var isCheckout=root.TryGetProperty("checkout",out var data);
        if(!isCheckout&&!root.TryGetProperty("payment",out data))throw new InvalidOperationException("Evento sem entidade.");
        var reference=data.Text("externalReference");var localId=reference?.StartsWith("csp_")==true?reference[4..]:null;
        var remotePayment=isCheckout?null:data.Text("id");var checkout=isCheckout?data.Text("id"):data.Text("checkoutSession");
        int tenant,order;
        using(var find=new MySqlCommand("SELECT TenantId,OrderId FROM paymentattempts WHERE AccountKey=@account AND (Id=@local OR RemotePaymentId=@payment OR RemoteCheckoutId=@checkout) LIMIT 2",c))
        {
            find.Parameters.AddWithValue("@account",account);find.Parameters.AddWithValue("@local",localId);find.Parameters.AddWithValue("@payment",remotePayment);find.Parameters.AddWithValue("@checkout",checkout);
            await using var r=await find.ExecuteReaderAsync(ct);if(!await r.ReadAsync(ct))throw new InvalidOperationException("Evento sem vínculo local.");
            tenant=r.GetInt32(0);order=r.GetInt32(1);if(await r.ReadAsync(ct))throw new InvalidOperationException("Evento ambíguo.");
        }
        await using var db=Context(sp,tenant);
        await using(var lease=await PaymentLock.Acquire(inbox.ConnectionString,"payment:"+order,ct))
        {
            var a=await db.Set<PaymentAttempt>().SingleAsync(x=>x.OrderId==order,ct);
            if(type.Contains("REFUND",StringComparison.Ordinal))a.RefundObserved=true;
            if(isCheckout)
            {
                if(a.Method!="CREDIT_CARD"||string.IsNullOrEmpty(checkout)||a.RemoteCheckoutId!=null&&a.RemoteCheckoutId!=checkout)throw new InvalidOperationException("Checkout divergente.");
                a.RemoteCheckoutId=checkout;
                var environment=await db.Set<AsaasConnection>().Where(x=>x.Id==a.ConnectionId).Select(x=>x.Environment).SingleAsync(ct);
                a.PaymentUrl=JsonFields.SafeUrl(data.Text("link"),environment)??a.PaymentUrl??JsonFields.CheckoutUrl(environment,checkout);
            }
            else if(remotePayment!=null)
            {
                if(a.RemotePaymentId!=null&&a.RemotePaymentId!=remotePayment)throw new InvalidOperationException("Cobrança duplicada.");
                a.RemotePaymentId=remotePayment;
            }
            await db.SaveChangesAsync(ct);
        }
        // Financial state is read from the authenticated account API, never from browser callbacks.
        await Service(sp,db).Reconcile(order,null,ct);
        if(type is "CHECKOUT_CANCELED" or "CHECKOUT_EXPIRED" or "PAYMENT_DELETED")
        {
            await using var lease=await PaymentLock.Acquire(inbox.ConnectionString,"payment:"+order,ct);db.ChangeTracker.Clear();
            var a=await db.Set<PaymentAttempt>().SingleAsync(x=>x.OrderId==order,ct);
            var o=await db.Orders.SingleAsync(x=>x.Id==order,ct);
            // An authenticated terminal event closes an unpaid journey; paid orders are never released by it.
            if(o.FinancialState=="Unpaid" && ((isCheckout&&a.ProviderStatus=="CHECKOUT_NO_PAYMENT")||(type=="PAYMENT_DELETED"&&a.ProviderStatus=="NOT_FOUND")))
            {
                if(o.ReservationState=="Reserved")await new StoreCommerceService(db,sp.GetRequiredService<IDataProtectionProvider>(),config).Release(order,null,ct,paymentResolved:true);
                a.State="Cancelled";a.LastError=null;await db.SaveChangesAsync(ct);
            }
            else if(o.FinancialState=="Unpaid"&&a.State!="Cancelled")throw new InvalidOperationException("Confirmar encerramento financeiro antes de liberar a reserva.");
        }
        using var complete=new MySqlCommand("UPDATE asaasinbox SET ProcessedAt=UTC_TIMESTAMP(6),LastError=NULL WHERE Id=@id",c);complete.Parameters.AddWithValue("@id",id);await complete.ExecuteNonQueryAsync(ct);
    }
    private sealed record WorkerTenant(int Id):ITenantProvider {public int GetTenantId()=>Id;}
}
