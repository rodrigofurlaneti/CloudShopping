using CloudShopping.Tests.E2E.Infrastructure;
using OpenQA.Selenium;
using Xunit;
namespace CloudShopping.Tests.E2E.Flows;
public class AccessFlows : BrowserTest
{
    [Theory,Trait("Category","ReadOnly")]
    [InlineData("dashboard")][InlineData("departments")][InlineData("products")]
    [InlineData("customers")][InlineData("banners")][InlineData("order-sectors")]
    [InlineData("order-statuses")][InlineData("orders")][InlineData("shipping")]
    [InlineData("access")][InlineData("security")][InlineData("asaas")]
    [InlineData("payments")][InlineData("coupons")][InlineData("imports")]
    [InlineData("catalog-details")][InlineData("support")][InlineData("reviews")]
    [InlineData("notifications")]
    public void Anonymous_user_is_redirected_from_protected_route(string route)=>Scenario(()=>
    {Open("/admin/"+route);At("/admin/login");Visible(By.CssSelector("input[placeholder='Seu usuário']"));});

    [Fact,Trait("Category","ReadOnly")]
    public void Unknown_route_shows_not_found_and_link_to_store()=>Scenario(()=>
    {Open("/e2e-missing-"+Guid.NewGuid().ToString("N"));Contains("Página não encontrada");Click(By.LinkText("Voltar à loja"));At("/");});

    [Fact,Trait("Category","ReadOnly")]
    public void Empty_admin_login_is_blocked_by_browser_validation()=>Scenario(()=>
    {
        Open("/admin/login");Click(Button("Entrar no Backoffice"));
        Assert.Equal("/admin/login",new Uri(Browser.Url).AbsolutePath);
        Assert.NotEmpty(Browser.FindElements(By.CssSelector("input:invalid")));
    });
}

// These requirements have no reachable UI in App.tsx. They must never appear as passed.
public class UnavailableFlows
{
    [Fact(Skip="Falta interface: /admin/register exibe aviso de provisionamento e não renderiza RegisterCompany."),Trait("Category","PendingUI")]
    public void Register_company_and_login_as_its_administrator()=>throw new NotImplementedException();
    [Fact(Skip="Falta interface: App.tsx não possui rota de cadastro/edição de funcionários."),Trait("Category","PendingUI")]
    public void Register_employee_edit_and_deactivate()=>throw new NotImplementedException();
}
