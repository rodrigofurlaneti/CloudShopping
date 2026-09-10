using CloudShopping.Tests.E2E.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;
namespace CloudShopping.Tests.E2E.Flows;
public class ProductFlows : BrowserTest
{
    [Fact,Trait("Category","Mutation")]
    public void Create_department_then_product_and_verify_after_reload()=>Scenario(()=>
    {
        AdminLogin();var department=Unique;var product=Unique;var sku=Unique;
        Open("/admin/departments");Click(Button("Novo Departamento"));
        Fill(By.CssSelector("input[placeholder='Ex: Eletrônicos']"),department);Click(Button("Criar departamento"));
        Wait.Until(d=>!d.FindElements(Button("Criar departamento")).Any(x=>x.Displayed));
        Open("/admin/products");Click(Button("Novo Produto"));
        var select=Sibling("Departamento","select");
        Wait.Until(_=>new SelectElement(Visible(select)).Options.Any(o=>o.Text==department));
        new SelectElement(Visible(select)).SelectByText(department);
        Fill(Sibling("SKU","input"),sku);Fill(Sibling("Preço","input"),"25,50");
        Fill(Sibling("Nome do produto","input"),product);Fill(Sibling("Estoque inicial","input"),"10");
        Click(Button("Criar produto"));
        Wait.Until(d=>!d.FindElements(Button("Criar produto")).Any(x=>x.Displayed));
        Fill(By.CssSelector("input[placeholder='Buscar por nome ou SKU...']"),sku);
        ProductRow(sku,product);Reload();Fill(By.CssSelector("input[placeholder='Buscar por nome ou SKU...']"),sku);ProductRow(sku,product);
    });
    private static By Sibling(string label,string tag)=>By.XPath($"//label[normalize-space(.)={Literal(label)}]/following-sibling::{tag}[1]");
    private void ProductRow(string sku,string name)
    {var row=Visible(By.XPath($"//tbody/tr[contains(.,{Literal(sku)})]"));Assert.Contains(name,row.Text);}
}
