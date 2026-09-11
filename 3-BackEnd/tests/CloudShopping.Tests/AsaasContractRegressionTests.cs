using CloudShopping.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

public sealed partial class CommerceTests
{
    [Theory]
    [InlineData(false,"RefundPending")]
    [InlineData(true,"Unknown")]
    public async Task Refunded_status_without_completed_history_does_not_release_fulfillment(bool failLookup,string expected)
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,null,default);
        gateway.Status="REFUNDED";gateway.FailRefundLookup=failLookup;await Payments(db,gateway).Reconcile(id,null,default);
        Assert.Equal(expected,(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.True((await db.Orders.SingleAsync()).FulfillmentBlocked);
    }

    [Fact]
    public async Task Missing_checkout_ids_cannot_replace_a_matching_external_reference()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"CREDIT_CARD",default);
        gateway.CompleteCheckout();gateway.Status="CONFIRMED";gateway.OverrideReference="another-order";gateway.OmitCheckoutSession=true;
        var attempt=await db.Set<PaymentAttempt>().SingleAsync();attempt.RemoteCheckoutId=null;await db.SaveChangesAsync();
        await Payments(db,gateway).Reconcile(id,null,default);
        Assert.Equal("Review",(await db.Set<PaymentAttempt>().SingleAsync()).State);Assert.Empty(await db.Payments.ToListAsync());
    }

    [Theory]
    [InlineData("PIX", false)]
    [InlineData("BOLETO", true)]
    public async Task Registration_cancellation_is_sent_only_for_bank_slips(string method,bool expected)
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,method,default);
        Assert.Equal(expected,gateway.LastPaymentBody.TryGetProperty("daysAfterDueDateToRegistrationCancellation",out _));
        Assert.Equal(expected,gateway.LastPaymentBody.TryGetProperty("postalService",out _));
    }

    [Theory]
    [InlineData(true,"CancelPending",1)]
    [InlineData(false,"Cancelled",0)]
    public async Task Checkout_cancellation_requires_matching_terminal_response(bool empty,string state,int reserved)
    {
        await using var db=Db(1);var gateway=new FakeAsaas{EmptyCancelResponse=empty};var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"CREDIT_CARD",default);
        await Payments(db,gateway).Reconcile(id,customerId,default,"cancel");db.ChangeTracker.Clear();
        Assert.Equal(state,(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.Equal(reserved,(await db.Products.SingleAsync()).ReservedStock);
    }

    [Theory]
    [InlineData("BLOCKED_BY_VALUE_DIVERGENCE",0)]
    [InlineData("PROCESSING_REFUND",0)]
    [InlineData("REFUNDED",0)]
    [InlineData("DONE",10)]
    public async Task Invalid_split_blocks_fulfillment(string status,decimal fixedValue)
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway,"PlatformSplit");
        await Payments(db,gateway).Start(id,customerId,"PIX",default);
        gateway.OverrideSplits=JsonSerializer.SerializeToElement(new[]{new{walletId="33333333-3333-4333-8333-333333333333",percentualValue=90,status,fixedValue}});
        gateway.Status="RECEIVED";await Payments(db,gateway).Reconcile(id,null,default);
        Assert.Equal("Review",(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.True((await db.Orders.SingleAsync()).FulfillmentBlocked);Assert.Empty(await db.Payments.ToListAsync());
    }

    [Fact]
    public async Task Existing_external_refund_is_not_requested_again()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,null,default);
        gateway.OverrideRefunds=JsonSerializer.SerializeToElement(new[]{new{status="PENDING",value=115}});
        await Payments(db,gateway).Reconcile(id,null,default,"refund");
        Assert.Equal(0,gateway.RefundRequests);Assert.Equal("RefundPending",(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.True((await db.Orders.SingleAsync()).FulfillmentBlocked);
    }

    [Theory]
    [InlineData(true,true,"Review")]
    [InlineData(false,false,"Review")]
    [InlineData(false,true,"Refunded")]
    public async Task Refund_requires_complete_history_and_completed_returned_splits(bool hasMore,bool done,string expected)
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway,"PlatformSplit");
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="REFUNDED";
        gateway.RefundHasMore=hasMore;
        gateway.OverrideRefunds=JsonSerializer.SerializeToElement(new[]{new{status="DONE",value=115,refundedSplits=new[]{new{id="split_test",value=100,done}}}});
        await Payments(db,gateway).Reconcile(id,null,default);
        Assert.Equal(expected,(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.Equal(expected,(await db.Orders.SingleAsync()).FinancialState);
    }

    [Fact]
    public async Task Refund_in_progress_after_payment_preserves_pending_state()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,null,default);
        await Payments(db,gateway).Reconcile(id,null,default,"refund");
        gateway.Status="REFUND_IN_PROGRESS";await Payments(db,gateway).Reconcile(id,null,default);
        Assert.Equal("RefundPending",(await db.Set<PaymentAttempt>().SingleAsync()).State);
        Assert.Equal(1,gateway.RefundRequests);
        var order=await db.Orders.SingleAsync();Assert.True(order.FulfillmentBlocked);
        Assert.Throws<InvalidOperationException>(()=>order.SetFulfillment("Picking"));
    }
}

public sealed class AsaasJsonContractTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("{\"data\":null}")]
    [InlineData("null")]
    public void Malformed_list_is_not_treated_as_proof_of_no_payments_or_refunds(string json)
    {
        using var doc=JsonDocument.Parse(json);Assert.Throws<InvalidOperationException>(()=>doc.RootElement.Rows());
    }

    [Theory]
    [InlineData("{\"percentualValue\":null}")]
    [InlineData("{}")]
    [InlineData("null")]
    public void Optional_numeric_fields_accept_documented_nulls(string json)
    {
        using var doc=JsonDocument.Parse(json);Assert.Equal(0,doc.RootElement.Money("percentualValue"));
    }
}
