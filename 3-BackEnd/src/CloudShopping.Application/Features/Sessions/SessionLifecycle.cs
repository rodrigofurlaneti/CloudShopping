using System.Security.Cryptography;
using System.Text;
using CloudShopping.Application.Abstractions.Data;

namespace CloudShopping.Application.Features.Sessions;

public enum TenantResolutionFailure { None, Missing, Forbidden, NotFound }
public sealed record TenantResolution(int TenantId, TenantResolutionFailure Failure);

public sealed class SessionLifecycle(ISessionStore store, TimeProvider clock)
{
    public static string Stamp(string credential) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));

    public async Task<TenantResolution> ResolveTenant(int authenticatedTenant, int requestedTenant, int? routeTenant, CancellationToken ct)
    {
        if (authenticatedTenant > 0 && requestedTenant > 0 && authenticatedTenant != requestedTenant)
            return new(0, TenantResolutionFailure.Forbidden);
        var tenant = authenticatedTenant > 0 ? authenticatedTenant : requestedTenant;
        if (tenant <= 0) return new(0, TenantResolutionFailure.Missing);
        if (routeTenant.HasValue && routeTenant != tenant) return new(0, TenantResolutionFailure.Forbidden);
        return await store.TenantExists(tenant, ct)
            ? new(tenant, TenantResolutionFailure.None) : new(tenant, TenantResolutionFailure.NotFound);
    }

    public async Task<bool> Validate(int tenant, string? sessionId, int subject, string kind, CancellationToken ct)
    {
        if (tenant <= 0 || subject <= 0 || string.IsNullOrEmpty(sessionId) || kind is not ("Customer" or "Administrator")) return false;
        var session = await store.Find(tenant, sessionId, ct);
        if (session == null || session.TenantId != tenant || session.SubjectId != subject || session.Kind != kind ||
            session.RevokedAt != null || session.ExpiresAt <= clock.GetUtcNow().UtcDateTime || !await store.TenantExists(tenant, ct)) return false;
        var identity = await store.Identity(tenant, subject, kind, ct);
        return identity is { Active: true, Credential: not null } &&
            (kind != "Administrator" || identity.HasAdministrativeAccess) && session.CredentialStamp == Stamp(identity.Credential);
    }

    public async Task<StoredSession> Create(int tenant, int subject, string kind, string credential, CancellationToken ct)
    {
        if (tenant <= 0 || subject <= 0 || kind is not ("Customer" or "Administrator")) throw new ArgumentException("Identidade de sessão inválida.");
        var session = new StoredSession(Guid.NewGuid().ToString("N"), tenant, subject, kind, Stamp(credential), clock.GetUtcNow().UtcDateTime.AddHours(8), null);
        await store.Add(session, ct);
        return session;
    }

    public Task Revoke(int tenant, string? sessionId, CancellationToken ct) => string.IsNullOrEmpty(sessionId)
        ? Task.CompletedTask : store.Revoke(tenant, sessionId, ct);
}
