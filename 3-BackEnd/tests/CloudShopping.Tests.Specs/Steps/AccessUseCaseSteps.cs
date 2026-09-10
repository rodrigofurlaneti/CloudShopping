using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using MediatR;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class AccessUseCaseSteps
{
    private readonly Mock<IAccessRepository> repository=new(MockBehavior.Strict);
    private readonly Mock<IAccessEdit> edit=new(MockBehavior.Loose);
    private ReplaceProfilePermissionsCommand request=null!;private Result<Unit> result=null!;
    [Given(@"uma substituição de permissões na condição ""(.*)""")]
    public void GivenPermissions(string condition)
    {
        edit.Setup(x=>x.Commit(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        edit.Setup(x=>x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        repository.Setup(x=>x.BeginEdit(2,It.IsAny<CancellationToken>())).ReturnsAsync(edit.Object);
        repository.Setup(x=>x.ForUser(1,It.IsAny<CancellationToken>())).ReturnsAsync(condition=="sem autorização"?[]:["*"]);
        AccessProfile? profile=condition=="ausente"?null:new(2,condition=="administrador geral"?"Administrador Geral":"Operador",true,["catalog.read"]);
        repository.Setup(x=>x.Profile(2,It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        repository.Setup(x=>x.SavePermissions(1,2,It.IsAny<string[]>(),It.IsAny<string[]>(),It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        request=new(1,2,condition=="concorrente"?[]:["catalog.read"],condition=="sem alteração"?["catalog.read"]:[]);
    }
    [When("o caso de uso substitui as permissões")]
    public async Task Replace()=>result=await new ReplaceProfilePermissionsCommandHandler(repository.Object).Handle(request,default);
    [Then(@"a substituição retorna ""(.*)"" e registra (.*) alterações")]
    public void Verify(string code,int writes)
    {
        if(code=="sucesso")result.IsSuccess.Should().BeTrue();else result.Error.Code.Should().Be(code);
        repository.Verify(x=>x.SavePermissions(1,2,It.IsAny<string[]>(),It.IsAny<string[]>(),It.IsAny<CancellationToken>()),Times.Exactly(writes));
        edit.Verify(x=>x.Commit(It.IsAny<CancellationToken>()),Times.Exactly(code=="sucesso"?1:0));
        edit.Verify(x=>x.DisposeAsync(),Times.Once);
    }
}
