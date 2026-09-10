using CloudShopping.Tests.E2E.Infrastructure;
using OpenQA.Selenium;
using Xunit;
namespace CloudShopping.Tests.E2E.Flows;

public class DepartmentFlows : BrowserTest
{
    [Fact, Trait("Category","Mutation")]
    public void Create_edit_reload_and_delete_department()=>Scenario(()=>
    {
        AdminLogin();Open("/admin/departments");var name=Unique;var edited=name+" editado";
        Click(Button("Novo Departamento"));Fill(By.CssSelector("input[placeholder='Ex: Eletrônicos']"),name);
        Click(Button("Criar departamento"));
        Search(name);Row(name);Reload();Search(name);Row(name);
        Click(By.XPath($"//tr[.//td[contains(.,{Literal(name)})]]//button[@title='Editar departamento']"));
        Fill(By.CssSelector("input[placeholder='Ex: Eletrônicos']"),edited);Click(Button("Salvar alterações"));
        Row(edited);Reload();Search(edited);Row(edited);
        Click(By.XPath($"//tr[.//td[contains(.,{Literal(edited)})]]//button[@title='Excluir departamento']"));
        Click(Button("Excluir"));
        Wait.Until(d=>!d.FindElements(By.CssSelector("tbody tr")).Any(r=>r.Text.Contains(edited)));
        Reload();Search(edited);Contains("Nenhum departamento");
    });
    private void Search(string value)=>Fill(By.CssSelector("input[placeholder='Buscar por nome ou slug...']"),value);
    private void Row(string name)=>Visible(By.XPath($"//tbody/tr[.//td[contains(.,{Literal(name)})]]"));
}
