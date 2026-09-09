using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MySqlConnector;
using System.Text.Json;
using Xunit;

public sealed partial class CommerceTests
{
    private const string WebhookSecret="synthetic-webhook-secret-more-than-32-characters";
    private IConfiguration PaymentConfig=>new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
        ["ConnectionStrings:DefaultConnection"]=connection,["Asaas:StoreUrls:1"]="https://store.example.test",
        ["Asaas:Platform:Sandbox:ApiKey"]="synthetic-platform-api-key",
        ["Asaas:Platform:Sandbox:WebhookToken"]=WebhookSecret,
        ["Asaas:Platform:Sandbox:MerchantWallets:1"]="33333333-3333-4333-8333-333333333333",
        ["Asaas:Platform:Sandbox:MerchantPercent:1"]="90"
    }).Build();
    private AsaasPayments Payments(AppDbContext db,FakeAsaas gateway)=>new(db,gateway,new(db,gateway,protection,PaymentConfig),Service(db),PaymentConfig);
    private async Task<int> PaymentOrder(AppDbContext db,FakeAsaas gateway,string mode="Direct")
    {
        await new AsaasAccounts(db,gateway,protection,PaymentConfig).Configure(mode,"Sandbox","synthetic-direct-key",WebhookSecret,default);
        var preview=await Prepare(db);return (await Service(db).Confirm(customerId,Guid.NewGuid().ToString(),preview.Token,default)).Id;
    }
    [Fact]
    public async Task Asaas_direct_payment_replay_commits_stock_and_ledger_once()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);
        Assert.Equal("AwaitingPayment",(await db.Set<PaymentAttempt>().SingleAsync()).State);
        gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,customerId,default);await Payments(db,gateway).Reconcile(id,customerId,default);
        db.ChangeTracker.Clear();Assert.Equal(1,gateway.PaymentCreates);Assert.Single(await db.Payments.ToListAsync());
        Assert.Single(await db.StockMovements.ToListAsync());Assert.Equal(0,(await db.Products.SingleAsync()).PhysicalStock);
        Assert.Equal("Consumed",(await db.Orders.SingleAsync()).ReservationState);
    }
    [Fact]
    public async Task Split_uses_server_rules_and_never_customer_supplied_distribution()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway,"PlatformSplit");
        await Payments(db,gateway).Start(id,customerId,"BOLETO",default);
        var a=await db.Set<PaymentAttempt>().SingleAsync();var split=JsonDocument.Parse(a.SplitJson).RootElement[0];
        Assert.Equal(90,split.Money("percentualValue"));Assert.Equal("33333333-3333-4333-8333-333333333333",split.Text("walletId"));
        Assert.Equal(a.SplitJson,gateway.LastPaymentBody.GetProperty("split").GetRawText());
        gateway.Status="RECEIVED";await Payments(db,gateway).Reconcile(id,customerId,default);
        Assert.Equal("Paid",(await db.Orders.SingleAsync()).FinancialState);
    }
    [Fact]
    public async Task Response_lost_after_remote_creation_is_recovered_without_another_post()
    {
        await using var db=Db(1);var gateway=new FakeAsaas {LoseCreationResponse=true};var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);
        Assert.Equal("Unknown",(await db.Set<PaymentAttempt>().SingleAsync()).State);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);
        Assert.Equal(1,gateway.PaymentCreates);Assert.Equal("pay_test",(await db.Set<PaymentAttempt>().SingleAsync()).RemotePaymentId);
    }
    [Fact]
    public async Task Concurrent_start_uses_one_durable_attempt_and_one_remote_charge()
    {
        var gateway=new FakeAsaas();int id;await using(var setup=Db(1))id=await PaymentOrder(setup,gateway);
        await using var a=Db(1);await using var b=Db(1);
        await Task.WhenAll(Payments(a,gateway).Start(id,customerId,"PIX",default),Payments(b,gateway).Start(id,customerId,"PIX",default));
        Assert.Equal(1,gateway.PaymentCreates);await using var verify=Db(1);Assert.Single(await verify.Set<PaymentAttempt>().ToListAsync());
    }
    [Fact]
    public async Task Unknown_creation_does_not_release_inventory_on_expiry()
    {
        await using var db=Db(1);var gateway=new FakeAsaas {LoseCreationResponse=true,HidePayments=true};var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);
        await db.Database.ExecuteSqlRawAsync("UPDATE orders SET ReservationExpiresAt=UTC_TIMESTAMP(6)-INTERVAL 1 MINUTE");
        await Payments(db,gateway).Reconcile(id,customerId,default);
        db.ChangeTracker.Clear();Assert.Equal(1,(await db.Products.SingleAsync()).ReservedStock);
        Assert.NotEqual("Cancelled",(await db.Set<PaymentAttempt>().SingleAsync()).State);Assert.Equal(1,gateway.PaymentCreates);
    }
    [Fact]
    public async Task Amount_mismatch_blocks_fulfillment_without_stock_debit()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.OverrideAmount=999;gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,customerId,default);db.ChangeTracker.Clear();
        Assert.True((await db.Orders.SingleAsync()).FulfillmentBlocked);Assert.Equal(1,(await db.Products.SingleAsync()).PhysicalStock);
        Assert.Empty(await db.Payments.ToListAsync());
    }
    [Fact]
    public async Task Refund_is_requested_once_and_never_replenishes_physical_stock()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,customerId,default);
        await Payments(db,gateway).Reconcile(id,null,default,"refund");await Payments(db,gateway).Reconcile(id,null,default,"refund");
        Assert.Equal(1,gateway.RefundRequests);gateway.Status="REFUNDED";
        await Payments(db,gateway).Reconcile(id,customerId,default);await Payments(db,gateway).Reconcile(id,customerId,default);
        db.ChangeTracker.Clear();Assert.Equal("Refunded",(await db.Orders.SingleAsync()).FinancialState);Assert.Equal(0,(await db.Products.SingleAsync()).PhysicalStock);
    }
    [Fact]
    public async Task Payment_after_cancel_is_financial_review_without_reselling_stock()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);await Payments(db,gateway).Reconcile(id,customerId,default,"cancel");
        gateway.Deleted=false;gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,customerId,default);db.ChangeTracker.Clear();
        Assert.Equal("PaidReview",(await db.Orders.SingleAsync()).FinancialState);Assert.True((await db.Orders.SingleAsync()).FulfillmentBlocked);
        Assert.Equal(1,(await db.Products.SingleAsync()).PhysicalStock);Assert.Equal(0,(await db.Products.SingleAsync()).ReservedStock);
    }
    [Fact]
    public async Task Webhook_duplicate_is_persisted_once_and_reconciled_with_provider()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        var account=await db.Set<AsaasConnection>().SingleAsync();var a=await db.Set<PaymentAttempt>().SingleAsync();
        var payload=JsonSerializer.Serialize(new{id="evt_same",@event="PAYMENT_RECEIVED",payment=new{id="pay_test",externalReference=JsonFields.Reference(a)}});
        var inbox=new AsaasInbox(PaymentConfig,protection);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>inbox.Receive(account.Id,"incorrect-secret-with-more-than-32-characters",payload,default));
        await inbox.Receive(account.Id,WebhookSecret,payload,default);await inbox.Receive(account.Id,WebhookSecret,payload,default);
        await RunPaymentWorker(gateway);await RunPaymentWorker(gateway);
        db.ChangeTracker.Clear();Assert.Single(await db.Payments.ToListAsync());Assert.Single(await db.StockMovements.ToListAsync());
        await using var c=new MySqlConnection(connection);await c.OpenAsync();
        Assert.Equal(1L,await new MySqlCommand("SELECT COUNT(*) FROM asaasinbox WHERE ProcessedAt IS NOT NULL",c).ExecuteScalarAsync());
    }
    [Fact]
    public async Task Hosted_card_checkout_is_not_paid_until_provider_confirms_charge()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"CREDIT_CARD",default);
        Assert.Equal("Unpaid",(await db.Orders.SingleAsync()).FinancialState);Assert.Equal(1,gateway.CheckoutCreates);
        gateway.CompleteCheckout();gateway.Status="CONFIRMED";
        await Payments(db,gateway).Reconcile(id,customerId,default);
        Assert.Equal("Paid",(await db.Orders.SingleAsync()).FinancialState);Assert.Equal(0,(await db.Products.SingleAsync()).PhysicalStock);
    }
    [Fact]
    public async Task Payment_owner_and_tenant_are_checked_before_any_external_call()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Assert.ThrowsAsync<KeyNotFoundException>(()=>Payments(db,gateway).Start(id,customerId+1,"PIX",default));
        await using var other=Db(2);await Assert.ThrowsAsync<KeyNotFoundException>(()=>Payments(other,gateway).Start(id,customerId,"PIX",default));
        Assert.Equal(0,gateway.PaymentCreates);
        var account=await db.Set<AsaasConnection>().SingleAsync();Assert.DoesNotContain("synthetic-direct-key",account.ProtectedApiKey);
    }
    private async Task RunPaymentWorker(FakeAsaas gateway)
    {
        var services=new ServiceCollection();services.AddSingleton<IAsaasGateway>(gateway);services.AddSingleton<IDataProtectionProvider>(protection);
        services.AddSingleton(new DbContextOptionsBuilder<AppDbContext>().UseMySql(connection,new MySqlServerVersion(new Version(8,0,43))).Options);
        services.AddSingleton(new AsaasInbox(PaymentConfig,protection));await using var provider=services.BuildServiceProvider();
        using var worker=new AsaasWorker(provider,PaymentConfig,NullLogger<AsaasWorker>.Instance);await worker.RunOnce(default);
    }
}

public sealed class FakeAsaas:IAsaasGateway
{
    public int PaymentCreates,CheckoutCreates,RefundRequests;
    public bool LoseCreationResponse,HidePayments,Deleted;
    public decimal? OverrideAmount;
    public string Status="PENDING";
    public JsonElement LastPaymentBody,LastCheckoutBody;
    private object? customer;
    private bool hasPayment;
    private static JsonElement Json(object value)=>JsonSerializer.SerializeToElement(value);
    public void CompleteCheckout()
    {
        LastPaymentBody=Json(new {customer=LastCheckoutBody.Text("customer"),billingType="CREDIT_CARD",value=LastCheckoutBody.GetProperty("items")[0].Money("value"),
            externalReference=LastCheckoutBody.Text("externalReference"),split=LastCheckoutBody.GetProperty("splits")});hasPayment=true;
    }
    private JsonElement Payment()=>Json(new{id="pay_test",customer=LastPaymentBody.Text("customer"),billingType=LastPaymentBody.Text("billingType"),value=OverrideAmount??LastPaymentBody.Money("value"),
        externalReference=LastPaymentBody.Text("externalReference"),checkoutSession=CheckoutCreates>0?"checkout_test":null,status=Status,deleted=Deleted,
        split=LastPaymentBody.GetProperty("split"),invoiceUrl="https://sandbox.asaas.com/i/test",bankSlipUrl="https://sandbox.asaas.com/b/pdf/test"});
    public Task<JsonElement> Send(AsaasConnection account,HttpMethod method,string path,object? body,CancellationToken ct)
    {
        if(path=="wallets/")return Task.FromResult(Json(new{data=new[]{new{id="11111111-1111-4111-8111-111111111111"}}}));
        if(path=="customers"&&method==HttpMethod.Post){customer=new{id="cus_test"};return Task.FromResult(Json(customer));}
        if(path.StartsWith("customers?")&&method==HttpMethod.Get)return Task.FromResult(Json(new{data=customer==null?Array.Empty<object>():new[]{customer}}));
        if(path=="customers/cus_test"&&method==HttpMethod.Put)return Task.FromResult(Json(customer!));
        if(path=="payments"&&method==HttpMethod.Post)
        {
            PaymentCreates++;LastPaymentBody=Json(body!);hasPayment=true;
            if(LoseCreationResponse)throw new HttpRequestException("synthetic response lost");
            return Task.FromResult(Payment());
        }
        if(path=="checkouts"&&method==HttpMethod.Post){CheckoutCreates++;LastCheckoutBody=Json(body!);return Task.FromResult(Json(new{id="checkout_test"}));}
        if(path.StartsWith("payments?")&&method==HttpMethod.Get)return Task.FromResult(Json(new{data=hasPayment&&!HidePayments?new[]{Payment()}:Array.Empty<JsonElement>()}));
        if(path=="payments/pay_test"&&method==HttpMethod.Get)return Task.FromResult(Payment());
        if(path=="payments/pay_test/pixQrCode")return Task.FromResult(Json(new{payload="synthetic-pix-code",encodedImage=""}));
        if(path=="payments/pay_test/refunds")return Task.FromResult(Json(new{data=RefundRequests==0?Array.Empty<object>():new object[]{new {id="ref_test",status=Status=="REFUNDED"?"DONE":"PENDING",value=LastPaymentBody.Money("value"),requestUrl="https://sandbox.asaas.com/refund/test"}}}));
        if(path=="payments/pay_test/bankSlip/refund"){RefundRequests++;return Task.FromResult(Json(new{requestUrl="https://sandbox.asaas.com/refund/test"}));}
        if(path=="payments/pay_test/refund"){RefundRequests++;Status="REFUND_REQUESTED";return Task.FromResult(Payment());}
        if(path=="payments/pay_test"&&method==HttpMethod.Delete){Deleted=true;return Task.FromResult(Json(new{deleted=true}));}
        if(path=="checkouts/checkout_test/cancel")return Task.FromResult(Json(new{}));
        throw new InvalidOperationException("Unexpected fake Asaas route: "+method+" "+path);
    }
}
