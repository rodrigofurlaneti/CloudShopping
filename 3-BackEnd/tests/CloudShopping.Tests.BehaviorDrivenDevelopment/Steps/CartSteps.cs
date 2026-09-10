using CloudShopping.Domain.Entities.Carts;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CartSteps
{
    private Cart cart=null!;private Exception? error;
    [Given(@"um carrinho com (.*) unidades do produto 10 a (.*) reais")]
    public void GivenCart(int quantity,decimal price) { cart=Cart.Create(1);cart.AddOrUpdateItem(10,quantity,price); }
    [When(@"o cliente executa ""(.*)"" com quantidade (.*) no carrinho")]
    public void Change(string operation,int quantity)
    { try { switch(operation){case "somar":cart.AddOrUpdateItem(10,quantity,20);break;case "definir":cart.SetQuantity(10,quantity);break;case "remover":cart.RemoveItem(10);break;case "limpar":cart.Clear();break;case "ausente":cart.SetQuantity(99,quantity);break;default:throw new NotSupportedException(operation);} }catch(Exception e){error=e;} }
    [Then(@"o carrinho deve ter (.*) unidades e total (.*) com resultado ""(.*)""")]
    public void ThenCart(int quantity,decimal total,string result)
    { if(result=="aceito")error.Should().BeNull();else error.Should().NotBeNull();cart.Items.Sum(x=>x.Quantity).Should().Be(quantity);cart.Items.Sum(x=>x.Quantity*x.UnitPrice).Should().Be(total); }
}
