namespace CloudShopping.Application.Abstractions.Data;

public sealed record StoredSession(string Id, int TenantId, int SubjectId, string Kind,
    string CredentialStamp, DateTime ExpiresAt, DateTime? RevokedAt);
public sealed record SessionIdentity(string? Credential, bool Active, bool HasAdministrativeAccess);

public interface ISessionStore
{
    Task<bool> TenantExists(int tenantId, CancellationToken ct);
    Task<StoredSession?> Find(int tenantId, string sessionId, CancellationToken ct);
    Task<SessionIdentity?> Identity(int tenantId, int subjectId, string kind, CancellationToken ct);
    Task Add(StoredSession session, CancellationToken ct);
    Task Revoke(int tenantId, string sessionId, CancellationToken ct);
}
