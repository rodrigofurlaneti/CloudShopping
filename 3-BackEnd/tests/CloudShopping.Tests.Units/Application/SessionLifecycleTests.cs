using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Sessions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class SessionLifecycleTests
{
    private sealed class Clock : TimeProvider
    { public override DateTimeOffset GetUtcNow()=>new(2026,1,1,12,0,0,TimeSpan.Zero); }
    private readonly Clock clock=new();
    private readonly Mock<ISessionStore> store=new(MockBehavior.Strict);

    [Theory]
    [InlineData(1,2,null,TenantResolutionFailure.Forbidden)]
    [InlineData(1,1,2,TenantResolutionFailure.Forbidden)]
    [InlineData(0,0,null,TenantResolutionFailure.Missing)]
    public async Task Invalid_tenant_resolution_never_reads_storage(int authenticated,int requested,int? route,TenantResolutionFailure failure)
    {
        var result=await new SessionLifecycle(store.Object,clock).ResolveTenant(authenticated,requested,route,default);
        result.Failure.Should().Be(failure);result.TenantId.Should().Be(0);store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Tenant_resolution_checks_existence(bool exists)
    {
        store.Setup(x=>x.TenantExists(1,default)).ReturnsAsync(exists);
        var result=await new SessionLifecycle(store.Object,clock).ResolveTenant(0,1,1,default);
        result.Failure.Should().Be(exists?TenantResolutionFailure.None:TenantResolutionFailure.NotFound);
        result.TenantId.Should().Be(1);
    }

    [Theory]
    [InlineData(0,1,"Customer")] [InlineData(1,0,"Customer")] [InlineData(1,1,"Unknown")]
    public async Task Invalid_identity_cannot_create_session(int tenant,int subject,string kind)
    {
        var service=new SessionLifecycle(store.Object,clock);
        Func<Task> act=()=>service.Create(tenant,subject,kind,"secret",default);
        await act.Should().ThrowAsync<ArgumentException>();store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Customer")] [InlineData("Administrator")]
    public async Task Created_session_expires_in_eight_hours_and_stores_only_credential_stamp(string kind)
    {
        StoredSession? saved=null;
        store.Setup(x=>x.Add(It.IsAny<StoredSession>(),default)).Callback<StoredSession,CancellationToken>((s,_)=>saved=s).Returns(Task.CompletedTask);
        var result=await new SessionLifecycle(store.Object,clock).Create(1,7,kind,"secret",default);
        result.Should().BeSameAs(saved);result.ExpiresAt.Should().Be(clock.GetUtcNow().UtcDateTime.AddHours(8));
        result.CredentialStamp.Should().Be(SessionLifecycle.Stamp("secret")).And.NotBe("secret");
        Guid.TryParseExact(result.Id,"N",out _).Should().BeTrue();result.RevokedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("valid",true)] [InlineData("expired",false)] [InlineData("revoked",false)]
    [InlineData("tenant",false)] [InlineData("subject",false)] [InlineData("kind",false)]
    [InlineData("credential",false)] [InlineData("inactive",false)] [InlineData("permission",false)]
    [InlineData("missing",false)] [InlineData("identity",false)] [InlineData("tenantMissing",false)]
    public async Task Validation_enforces_session_identity_and_expiration(string problem,bool expected)
    {
        var session=new StoredSession("session",1,7,"Administrator",SessionLifecycle.Stamp("secret"),clock.GetUtcNow().UtcDateTime.AddHours(1),null);
        session=problem switch {"expired"=>session with {ExpiresAt=clock.GetUtcNow().UtcDateTime},"revoked"=>session with {RevokedAt=clock.GetUtcNow().UtcDateTime},"tenant"=>session with {TenantId=2},"subject"=>session with {SubjectId=8},"kind"=>session with {Kind="Customer"},_=>session};
        store.Setup(x=>x.Find(1,"session",default)).ReturnsAsync(problem=="missing"?null:session);
        store.Setup(x=>x.TenantExists(1,default)).ReturnsAsync(problem!="tenantMissing");
        store.Setup(x=>x.Identity(1,7,"Administrator",default)).ReturnsAsync(problem=="identity"?null:new SessionIdentity(problem=="credential"?"changed":"secret",problem!="inactive",problem!="permission"));
        (await new SessionLifecycle(store.Object,clock).Validate(1,"session",7,"Administrator",default)).Should().Be(expected);
    }

    [Fact]
    public async Task Revocation_with_no_session_is_noop_otherwise_delegates_with_token()
    {
        using var cts=new CancellationTokenSource();var service=new SessionLifecycle(store.Object,clock);
        await service.Revoke(1,null,cts.Token);await service.Revoke(1,"",cts.Token);store.VerifyNoOtherCalls();
        store.Setup(x=>x.Revoke(1,"session",cts.Token)).Returns(Task.CompletedTask);
        await service.Revoke(1,"session",cts.Token);store.Verify(x=>x.Revoke(1,"session",cts.Token),Times.Once);
    }
}
