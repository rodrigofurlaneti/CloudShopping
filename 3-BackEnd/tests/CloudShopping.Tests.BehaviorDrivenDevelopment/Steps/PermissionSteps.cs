using CloudShopping.Domain.Entities.Backoffice;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class PermissionSteps
{
    private string[] desired=[];private string[]? normalized;private Exception? error;
    [Given(@"as permissões solicitadas ""(.*)""")]
    public void GivenPermissions(string permissions)=>desired=permissions.Split(',',StringSplitOptions.RemoveEmptyEntries);
    [When("a política valida o conjunto de permissões")]
    public void Normalize(){try{normalized=PermissionPolicy.Normalize(desired);}catch(Exception e){error=e;}}
    [Then(@"o conjunto de permissões é ""(.*)""")]
    public void Verify(string result){if(result=="aceito"){error.Should().BeNull();normalized.Should().OnlyHaveUniqueItems();}else error.Should().BeOfType<ArgumentException>();}
    [Then(@"a autorização para ""(.*)"" deve ser (.*)")]
    public void Access(string permission,bool allowed)=>PermissionPolicy.Allows(desired,permission).Should().Be(allowed);
}
