using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CustomerSteps
{
    private Customer customer=null!;private Exception? error;private string operation="";
    [Given(@"um cliente do tipo ""(.*)""")]
    public void GivenCustomer(string type){customer=Customer.CreateGuest(1);if(type=="B2C")customer.RegisterAsB2C("52998224725","Cliente",null);if(type=="B2B")customer.RegisterAsB2B("11222333000181","Empresa",null);}
    [When(@"o cliente solicita conversão para ""(.*)""")]
    public void Convert(string type){operation=type;try{if(type=="B2C")customer.RegisterAsB2C("52998224725","Cliente",null);else customer.RegisterAsB2B("11222333000181","Empresa",null);}catch(Exception e){error=e;}}
    [Then(@"o cliente permanece no tipo ""(.*)"" com resultado ""(.*)""")]
    public void Verify(string type,string result){customer.CustomerTypeId.ToString().Should().Be(type);if(result=="aceito")error.Should().BeNull();else error.Should().BeOfType<InvalidOperationException>();}
    [Then(@"o documento fiscal ""(.*)"" tem validade (.*)")]
    public void Document(string document,bool valid)=>TaxDocument.IsValid(document).Should().Be(valid);
}
