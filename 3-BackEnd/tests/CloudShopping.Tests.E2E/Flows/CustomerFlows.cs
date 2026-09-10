using CloudShopping.Tests.E2E.Infrastructure;
using OpenQA.Selenium;
using Xunit;
namespace CloudShopping.Tests.E2E.Flows;
public class CustomerFlows : BrowserTest
{
    [Fact,Trait("Category","Mutation")]
    public void Register_customer_persists_session_and_logout_removes_access()=>Scenario(()=>
    {
        RegisterCustomer();Open("/account");Contains("Sessão de");Reload();Contains("Sessão de");
        Click(Button("Sair desta conta"));Visible(By.Name("email"));Reload();Visible(By.Name("email"));
        Assert.Empty(Browser.FindElements(Button("Sair desta conta")));
    });
    [Fact,Trait("Category","Mutation")]
    public void Cart_add_increase_decrease_reload_and_remove()=>Scenario(()=>
    {
        RegisterCustomer();AddProduct();Open("/cart");Visible(By.CssSelector(".cart-line"));
        Click(By.CssSelector("button[aria-label^='Aumentar ' ]"));Quantity("2");Reload();Quantity("2");
        Click(By.CssSelector("button[aria-label^='Diminuir ' ]"));Quantity("1");
        Click(Button("Remover"));Contains("Seu carrinho está vazio");Reload();Contains("Seu carrinho está vazio");
    });
    private void Quantity(string expected)=>Wait.Until(d=>d.FindElement(By.CssSelector("span[aria-label='Quantidade']")).Text==expected);
    [Fact,Trait("Category","Mutation")]
    public void Support_ticket_and_reply_remain_after_reload()=>Scenario(()=>
    {
        RegisterCustomer();Open("/support");var subject=Unique;var message="Mensagem "+Unique;
        Fill(Label("Assunto"),subject);Fill(Label("Mensagem"),message);Click(Button("Enviar mensagem"));
        Visible(By.XPath($"//h2[normalize-space(.)={Literal(subject)}]"));Contains(message);
        var reply="Resposta "+Unique;Fill(Label("Mensagem"),reply);Click(Button("Enviar mensagem"));Contains(reply);
        Reload();Click(By.XPath($"//button[contains(.,{Literal(subject)})]"));Contains(message);Contains(reply);
    });
    [Fact,Trait("Category","Mutation")]
    public void Checkout_saves_buyer_address_and_creates_order_without_payment()=>Scenario(()=>
    {
        var email=RegisterCustomer();AddProduct();Open("/checkout");
        Fill(Label("Nome completo"),"Comprador E2E");Fill(Label("Email"),email);
        Fill(Label("CPF"),"52998224725");Click(Button("Salvar dados"));Contains("Dados do comprador salvos.");
        Fill(By.Name("zipCode"),"01001000");Fill(By.Name("street"),"Rua de Teste");Fill(By.Name("number"),"10");
        Fill(By.Name("neighborhood"),"Centro");Fill(By.Name("city"),"São Paulo");Fill(By.Name("state"),"SP");
        Click(Button("Salvar endereço"));Contains("Endereço salvo.");
        Click(Button("Revisar pedido"));Visible(Button("Confirmar pedido"));Click(Button("Confirmar pedido"));
        Wait.Until(d=>System.Text.RegularExpressions.Regex.IsMatch(new Uri(d.Url).AbsolutePath,@"^/orders/\d+$"));
        var url=Browser.Url;Contains("Pedido criado. Confira a situação financeira abaixo.");
        Visible(By.CssSelector(".order-items li"));Reload();Wait.Until(d=>d.Url==url);
        Contains("Pedido criado. Confira a situação financeira abaixo.");Visible(By.CssSelector(".order-items li"));
        Assert.Empty(Browser.FindElements(By.CssSelector("[role='alert']")));
        Click(Button("Solicitar cancelamento do pedido"));Contains("Pedido cancelado.");Reload();Contains("Pedido cancelado.");
    });
}
