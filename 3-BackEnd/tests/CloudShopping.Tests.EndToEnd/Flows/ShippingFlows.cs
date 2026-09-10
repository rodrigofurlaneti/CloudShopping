using CloudShopping.Tests.E2E.Infrastructure;
using OpenQA.Selenium;
using Xunit;
namespace CloudShopping.Tests.E2E.Flows;
public class ShippingFlows : BrowserTest
{
    [Fact,Trait("Category","Mutation")]
    public void Create_shipping_reload_and_deactivate()=>Scenario(()=>
    {
        AdminLogin();Open("/admin/shipping");var name=Unique;
        Fill(By.Name("name"),name);Fill(By.Name("amount"),"12");Fill(By.Name("postalCodePrefix"),"01001");Fill(By.Name("estimatedDays"),"3");
        Click(Button("Cadastrar entrega"));Shipping(name);Reload();Shipping(name);
        Click(By.XPath($"//article[.//h2[normalize-space(.)={Literal(name)}]]//button[normalize-space(.)='Desativar']"));
        Wait.Until(_=>Shipping(name).Text.Contains("Desativada"));Reload();
        Wait.Until(_=>Shipping(name).Text.Contains("Desativada"));
    });
    private IWebElement Shipping(string name)=>Visible(By.XPath($"//article[.//h2[normalize-space(.)={Literal(name)}]]"));
}
