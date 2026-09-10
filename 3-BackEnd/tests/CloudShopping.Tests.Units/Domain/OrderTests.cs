using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CloudShopping.Tests.Units.Domain;
public class OrderTests
{
    private static Order Create() => Order.Checkout(1,7,new[]{(2,2,50m)},
        Address.Create(7,(AddressType)1,"Rua","10",null,"Cidade","SP","01001000",true));

    [Fact]
    public void Shipping_and_discount_are_included_in_total()
    {
        var order=Create();order.TotalAmount.Should().Be(100);
        order.ConfigureCheckout("key","hash",12,"Entrega",DateTime.UtcNow.AddHours(1));
        order.ApplyCoupon("PROMO",10);
        order.TotalAmount.Should().Be(102);order.ShippingAmount.Should().Be(12);
        order.DiscountAmount.Should().Be(10);order.CouponCode.Should().Be("PROMO");
        order.ReservationState.Should().Be("Reserved");order.OrderItems.Should().ContainSingle();
    }

    [Theory]
    [InlineData(-1)] [InlineData(100)] [InlineData(101)]
    public void Invalid_discount_preserves_total(decimal discount)
    {
        var order=Create();var act=()=>order.ApplyCoupon("PROMO",discount);
        act.Should().Throw<ArgumentException>();order.TotalAmount.Should().Be(100);
    }

    [Fact]
    public void Provider_payment_is_idempotent_and_consumes_reservation()
    {
        var order=Create();order.ConfigureCheckout("key","hash",0,"Entrega",DateTime.UtcNow.AddHours(1));
        order.SchedulePayment(DateTime.UtcNow.AddDays(1));
        order.RecordProviderPayment("pay_1","PIX",100);order.RecordProviderPayment("pay_1","PIX",100);
        order.Payments.Should().ContainSingle();order.FinancialState.Should().Be("Paid");
        order.ReservationState.Should().Be("Consumed");order.FulfillmentBlocked.Should().BeFalse();
        order.SetFulfillment("Processing");order.FulfillmentState.Should().Be("Processing");
        order.RecordProviderRefund("pay_1","PIX",100);
        order.FinancialState.Should().Be("Refunded");order.FulfillmentBlocked.Should().BeTrue();
    }

    [Fact]
    public void Payment_after_release_requires_financial_review()
    {
        var order=Create();order.ConfigureCheckout("key","hash",0,"Entrega",DateTime.UtcNow.AddHours(1));
        order.ReleaseReservation();order.RecordProviderPayment("pay_1","PIX",100);
        order.FinancialState.Should().Be("PaidReview");order.FulfillmentBlocked.Should().BeTrue();
        var act=()=>order.SetFulfillment("Picking");act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Paid_order_progresses_through_logistics_to_return()
    {
        var order=Create();order.AddApprovedPayment("PIX",100);
        order.MarkAsInvoiced();order.StartProcessing();order.StartSeparating();order.StartPacking();
        order.GenerateShippingLabel();order.MarkAsReadyToShip();order.ShipOrder();
        order.SetTrackingNumber();order.MarkAsIntransit();order.MarkAsDelivered();
        order.OrderStatusId.Should().Be((int)OrderStatusEnum.Delivered);
        order.RequestReturn("Defeito");order.OrderStatusId.Should().Be((int)OrderStatusEnum.Returning);
        order.StateHistory.Last().Notes.Should().Contain("Defeito");
    }

    [Theory]
    [InlineData("invoice")] [InlineData("process")] [InlineData("pick")]
    [InlineData("pack")] [InlineData("label")] [InlineData("ready")]
    [InlineData("ship")] [InlineData("tracking")] [InlineData("transit")]
    [InlineData("delivered")] [InlineData("failed")] [InlineData("return")]
    public void Pending_order_cannot_skip_logistics_prerequisites(string operation)
    {
        var order=Create();Action act=()=> {switch(operation) {
            case "invoice":order.MarkAsInvoiced();break;case "process":order.StartProcessing();break;
            case "pick":order.StartSeparating();break;case "pack":order.StartPacking();break;
            case "label":order.GenerateShippingLabel();break;case "ready":order.MarkAsReadyToShip();break;
            case "ship":order.ShipOrder();break;case "tracking":order.SetTrackingNumber();break;
            case "transit":order.MarkAsIntransit();break;case "delivered":order.MarkAsDelivered();break;
            case "failed":order.MarkAsDeliveryFailed();break;case "return":order.RequestReturn("Defeito");break;
        }};
        act.Should().Throw<InvalidOperationException>();order.OrderStatusId.Should().Be((int)OrderStatusEnum.Pending);
        order.StateHistory.Should().ContainSingle();
    }
}
