using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Sessions;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Backoffice;
using FluentAssertions;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class AuthenticationSteps
{
    private readonly Mock<ISessionAccounts> accounts=new(MockBehavior.Strict);
    private readonly Mock<ISessionStore> store=new(MockBehavior.Strict);
    private readonly Mock<IPasswordHasher> hasher=new(MockBehavior.Strict);
    private readonly Mock<ITenantProvider> tenant=new(MockBehavior.Strict);
    private string condition="";private SessionLogin? result;private Exception? error;private Customer customer=null!;private SessionUseCases service=null!;
    [Given(@"uma autenticação na condição ""(.*)""")]
    public void GivenAuthentication(string status)
    {
        condition=status;tenant.Setup(x=>x.GetTenantId()).Returns(1);
        customer=Customer.CreateGuest(1);typeof(Customer).GetProperty("Id")!.SetValue(customer,10);customer.ChangeEmail("cliente@example.test");customer.SetPassword("hash");
        var employee=EmployeeUser.Create(1,1,"admin","hash");typeof(EmployeeUser).GetProperty("Id")!.SetValue(employee,20);
        accounts.Setup(x=>x.EmployeeLogin("admin",It.IsAny<CancellationToken>())).ReturnsAsync(status=="ausente"?null:employee);
        accounts.Setup(x=>x.EmployeeActive(1,It.IsAny<CancellationToken>())).ReturnsAsync(status!="funcionário inativo");
        accounts.Setup(x=>x.Permissions(20,It.IsAny<CancellationToken>())).ReturnsAsync(status=="sem permissões"?[]:["*"]);
        accounts.Setup(x=>x.CustomerByEmail("cliente@example.test",It.IsAny<CancellationToken>())).ReturnsAsync(status=="ausente"?null:customer);
        accounts.Setup(x=>x.EmailUsed("cliente@example.test",0,It.IsAny<CancellationToken>())).ReturnsAsync(status=="email duplicado");
        accounts.Setup(x=>x.AddCustomer(It.IsAny<Customer>())).Callback<Customer>(c=>typeof(Customer).GetProperty("Id")!.SetValue(c,30));
        accounts.Setup(x=>x.Save(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        hasher.Setup(x=>x.Verify(It.IsAny<string>(),"hash")).Returns(status!="senha incorreta");
        hasher.Setup(x=>x.Hash(It.IsAny<string>())).Returns("new-hash");
        store.Setup(x=>x.Add(It.IsAny<StoredSession>(),It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        service=new(accounts.Object,new SessionLifecycle(store.Object,TimeProvider.System),hasher.Object,tenant.Object,TimeProvider.System);
    }
    [When(@"o caso de uso executa ""(.*)""")]
    public async Task Execute(string operation)
    {
        var caller=new SessionCaller(0,condition=="sessão administrativa"?"Administrator":null,null);
        try{result=operation switch{
            "login administrativo"=>await service.AdminLogin(caller," admin ","Password-2026!",default),
            "login cliente"=>await service.CustomerLogin(caller," CLIENTE@EXAMPLE.TEST ","Password-2026!",default),
            "cadastro"=>await service.Register(caller," CLIENTE@EXAMPLE.TEST ",condition=="senha curta"?"abc":condition=="senha excedida"?new string('a',73):"Password-2026!",default),
            "visitante"=>await service.Guest(caller,default),
            _=>throw new NotSupportedException(operation)};}catch(Exception e){error=e;}
    }
    [Then(@"a autenticação retorna ""(.*)"", grava conta (.*) vezes e emite sessão (.*) vezes")]
    public void Verify(string expected,int saves,int sessions)
    {
        switch(expected){case "aceita":error.Should().BeNull();result.Should().NotBeNull();result!.Session!.TenantId.Should().Be(1);break;case "credenciais inválidas":error.Should().BeNull();result.Should().BeNull();break;case "proibida":error.Should().BeOfType<UnauthorizedAccessException>();break;case "conflito":error.Should().BeOfType<InvalidOperationException>();break;case "dados inválidos":error.Should().BeOfType<ArgumentException>();break;default:throw new NotSupportedException(expected);}
        accounts.Verify(x=>x.Save(It.IsAny<CancellationToken>()),Times.Exactly(saves));
        store.Verify(x=>x.Add(It.IsAny<StoredSession>(),It.IsAny<CancellationToken>()),Times.Exactly(sessions));
    }
}
