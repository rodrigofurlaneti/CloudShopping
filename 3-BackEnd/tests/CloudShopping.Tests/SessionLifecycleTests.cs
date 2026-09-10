using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Sessions;
using Xunit;

public sealed class SessionLifecycleTests
{
    private sealed class Store : ISessionStore
    {
        public StoredSession? Session { get; set; }
        public SessionIdentity? User { get; set; } = new("credential", true, true);
        public int TenantReads { get; private set; }
        public Task<bool> TenantExists(int tenantId, CancellationToken ct) { TenantReads++; return Task.FromResult(tenantId == 1); }
        public Task<StoredSession?> Find(int tenantId, string sessionId, CancellationToken ct) => Task.FromResult(Session);
        public Task<SessionIdentity?> Identity(int tenantId, int subjectId, string kind, CancellationToken ct) => Task.FromResult(User);
        public Task Add(StoredSession session, CancellationToken ct) { Session = session; return Task.CompletedTask; }
        public Task Revoke(int tenantId, string sessionId, CancellationToken ct) => Task.CompletedTask;
    }

    [Theory]
    [InlineData(1, 2, null)]
    [InlineData(1, 1, 2)]
    public async Task Tenant_mismatch_is_rejected_before_persistence(int authenticated, int requested, int? route)
    {
        var store = new Store(); var service = new SessionLifecycle(store, TimeProvider.System);
        Assert.Equal(TenantResolutionFailure.Forbidden, (await service.ResolveTenant(authenticated, requested, route, default)).Failure);
        Assert.Equal(0, store.TenantReads);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("revoked")]
    [InlineData("tenant")]
    [InlineData("subject")]
    [InlineData("role")]
    [InlineData("credential")]
    [InlineData("inactive")]
    [InlineData("permission")]
    public async Task Session_validation_rejects_invalid_identity_or_lifecycle(string problem)
    {
        var store = new Store(); var service = new SessionLifecycle(store, TimeProvider.System);
        var valid = await service.Create(1, 10, "Administrator", "credential", default);
        Assert.True(await service.Validate(1, valid.Id, 10, "Administrator", default));
        store.Session = problem switch
        {
            "expired" => valid with { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) },
            "revoked" => valid with { RevokedAt = DateTime.UtcNow },
            "tenant" => valid with { TenantId = 2 },
            "subject" => valid with { SubjectId = 20 },
            "role" => valid with { Kind = "Customer" },
            _ => valid
        };
        store.User = problem switch
        {
            "credential" => new("changed", true, true),
            "inactive" => new("credential", false, true),
            "permission" => new("credential", true, false),
            _ => store.User
        };
        Assert.False(await service.Validate(1, valid.Id, 10, "Administrator", default));
    }
}
