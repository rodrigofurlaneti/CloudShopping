using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class OrderSteps
{
    private Order order=null!;private Exception? error;
    private static Address Address()=>CloudShopping.Domain.Entities.Customers.Address.Create(1,AddressType.Shipping,"Rua","1","Centro","São Paulo","SP","01001000",true);
    [Given(@"um pedido de 100 reais com frete 15 e reserva ""(.*)""")]
    public void GivenOrder(string state){order=Order.Checkout(1,1,[(10,1,100m)],Address());order.ConfigureCheckout("key","hash",15,"Entrega",DateTime.UtcNow.AddHours(1));if(state=="liberada")order.ReleaseReservation();}
    [When(@"o provedor confirma o pagamento ""(.*)"" (.*) vezes")]
    public void Pay(string key,int count){for(var i=0;i<count;i++)order.RecordProviderPayment(key,"PIX",115);}
    [When("o provedor confirma o estorno")]
    public void Refund()=>order.RecordProviderRefund("pay-1","PIX",115);
    [Then(@"o pedido fica financeiro ""(.*)"", reserva ""(.*)"" e bloqueado (.*)")]
    public void Verify(string financial,string reservation,bool blocked){order.FinancialState.Should().Be(financial);order.ReservationState.Should().Be(reservation);order.FulfillmentBlocked.Should().Be(blocked);}
    [Then(@"o pedido possui (.*) pagamentos")]
    public void Payments(int count)=>order.Payments.Should().HaveCount(count);
    [When(@"a operação logística ""(.*)"" é solicitada")]
    public void Logistics(string action){try{switch(action){case "faturar":order.MarkAsInvoiced();break;case "processar":order.StartProcessing();break;case "separar":order.StartSeparating();break;case "embalar":order.StartPacking();break;case "etiquetar":order.GenerateShippingLabel();break;case "despachar":order.ShipOrder();break;default:throw new NotSupportedException(action);}}catch(Exception e){error=e;}}
    [Then("a etapa logística deve ser recusada sem mudar o pedido pendente")]
    public void Rejected(){error.Should().BeOfType<InvalidOperationException>();order.OrderStatusId.Should().Be(1);}
    [When(@"o desconto aplicado no pedido é (.*)")]
    public void Discount(decimal amount){try{order.ApplyCoupon("PROMO",amount);}catch(Exception e){error=e;}}
    [Then(@"o pedido totaliza (.*) e o desconto é ""(.*)""")]
    public void Total(decimal amount,string result){order.TotalAmount.Should().Be(amount);if(result=="aceito")error.Should().BeNull();else error.Should().BeOfType<ArgumentException>();}
}
