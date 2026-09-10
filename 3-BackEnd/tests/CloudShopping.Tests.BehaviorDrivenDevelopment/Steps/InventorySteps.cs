using CloudShopping.Domain.Entities.Products;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class InventorySteps
{
    private Product product = null!;
    private Exception? error;
    [Given(@"um produto com estoque físico (.*) e reserva (.*)")]
    public void GivenStock(int physical,int reserved) { product=Product.Create(1,1,"SKU","Produto",100,physical); if(reserved>0)product.ReserveStock(reserved); }
    [When(@"o estoque recebe a operação ""(.*)"" com quantidade (.*)")]
    public void Operate(string operation,int quantity)
    {
        try { switch(operation) { case "reservar":product.ReserveStock(quantity);break;case "baixar":product.CommitReservedStock(quantity);break;case "liberar":product.ReleaseReservedStock(quantity);break;case "ajustar":product.AdjustInventory(quantity);break;case "entrada":product.AddPhysicalStock(quantity);break;default:throw new NotSupportedException(operation); } }
        catch(Exception e) { error=e; }
    }
    [Then(@"o estoque termina físico (.*), reservado (.*), disponível (.*) e resultado ""(.*)""")]
    public void Verify(int physical,int reserved,int available,string result)
    {
        if(result=="aceito")error.Should().BeNull();else error.Should().BeAssignableTo<Exception>();
        product.PhysicalStock.Should().Be(physical);product.ReservedStock.Should().Be(reserved);product.AvailableStock.Should().Be(available);
    }
}
