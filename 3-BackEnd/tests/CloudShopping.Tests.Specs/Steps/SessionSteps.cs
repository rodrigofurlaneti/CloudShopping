using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Sessions;
using FluentAssertions;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class SessionSteps
{
    private readonly Mock<ISessionStore> store=new(MockBehavior.Strict);
    private SessionLifecycle service=null!;private bool valid;private TenantResolution? resolution;
    private readonly DateTime now=new(2026,9,10,12,0,0,DateTimeKind.Utc);
    private sealed class Clock(DateTime now) : TimeProvider { public override DateTimeOffset GetUtcNow()=>new(now); }
    [Given(@"uma sessão administrativa na condição ""(.*)""")]
    public void GivenSession(string condition)
    {
        StoredSession? session=new("session",1,10,"Administrator",SessionLifecycle.Stamp("password"),now.AddHours(1),null);
        var identity=new SessionIdentity("password",true,true);
        session=condition switch {"ausente"=>null,"expirada"=>session with{ExpiresAt=now},"revogada"=>session with{RevokedAt=now},"outra loja"=>session with{TenantId=2},"outro titular"=>session with{SubjectId=20},"outro papel"=>session with{Kind="Customer"},_=>session};
        identity=condition switch {"senha alterada"=>new("new-password",true,true),"inativa"=>new("password",false,true),"sem permissão"=>new("password",true,false),_=>identity};
        store.Setup(x=>x.Find(1,"session",It.IsAny<CancellationToken>())).ReturnsAsync(session);
        store.Setup(x=>x.TenantExists(1,It.IsAny<CancellationToken>())).ReturnsAsync(condition!="loja inativa");
        store.Setup(x=>x.Identity(1,10,"Administrator",It.IsAny<CancellationToken>())).ReturnsAsync(identity);
        service=new(store.Object,new Clock(now));
    }
    [When("a sessão administrativa é validada")]
    public async Task Validate()=>valid=await service.Validate(1,"session",10,"Administrator",default);
    [Then(@"a sessão é aceita (.*)")]
    public void Verify(bool expected)=>valid.Should().Be(expected);
    [When(@"a loja é resolvida com autenticada (.*), solicitada (.*) e rota (.*)")]
    public async Task Resolve(int authenticated,int requested,int route)
    {
        store.Setup(x=>x.TenantExists(It.IsAny<int>(),It.IsAny<CancellationToken>())).ReturnsAsync((int id,CancellationToken _)=>id==1);
        service=new(store.Object,new Clock(now));resolution=await service.ResolveTenant(authenticated,requested,route==0?null:route,default);
    }
    [Then(@"a resolução da loja resulta em ""(.*)""")]
    public void Resolved(string expected){resolution!.Failure.ToString().Should().Be(expected);if(expected is "Forbidden" or "Missing")store.Verify(x=>x.TenantExists(It.IsAny<int>(),It.IsAny<CancellationToken>()),Times.Never);}
}
