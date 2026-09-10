using CloudShopping.Domain.Entities.Security;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudShopping.Infrastructure.Repositories;

public sealed class SessionStore(AppDbContext db) : ISessionStore
{
    public Task<bool> TenantExists(int tenantId, CancellationToken ct) => db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Id == tenantId && x.IsActive, ct);

    public Task<StoredSession?> Find(int tenantId, string sessionId, CancellationToken ct) => db.Set<AuthSession>().AsNoTracking()
        .Where(x => x.Id == sessionId && x.TenantId == tenantId)
        .Select(x => new StoredSession(x.Id, x.TenantId, x.SubjectId, x.Kind, x.CredentialStamp, x.ExpiresAt, x.RevokedAt)).SingleOrDefaultAsync(ct);

    public async Task<SessionIdentity?> Identity(int tenantId, int subjectId, string kind, CancellationToken ct)
    {
        if (kind == "Administrator")
        {
            var user = await db.Set<EmployeeUser>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, ct);
            if (user == null) return null;
            var employeeActive = await db.Set<Employee>().IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Id == user.EmployeeId && x.IsActive, ct);
            var profiles = from membership in db.Set<ProfileUser>().IgnoreQueryFilters()
                join profile in db.Set<Profile>().IgnoreQueryFilters() on membership.ProfileId equals profile.Id
                where membership.TenantId == tenantId && profile.TenantId == tenantId && membership.EmployeeUserId == subjectId && membership.IsActive && profile.IsActive
                select profile;
            var permitted = await profiles.AnyAsync(x => x.Name == "Administrador Geral", ct) ||
                await db.Set<ProfilePermission>().IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && profiles.Any(p => p.Id == x.ProfileId), ct);
            return new(user.PasswordHash, user.IsActive && employeeActive, permitted);
        }
        var customer = await db.Customers.IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, ct);
        return customer == null ? null : new(customer.PasswordHash ?? customer.SessionToken.ToString(), customer.IsActive, false);
    }

    public async Task Add(StoredSession session, CancellationToken ct)
    {
        var entity = AuthSession.Create(session.TenantId, session.SubjectId, session.Kind, session.CredentialStamp, session.ExpiresAt, session.Id);
        if (session.RevokedAt.HasValue) entity.Revoke(session.RevokedAt);
        db.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public async Task Revoke(int tenantId, string sessionId, CancellationToken ct)
    {
        var session = await db.Set<AuthSession>().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == sessionId, ct);
        if (session == null || session.RevokedAt != null) return;
        session.Revoke();
        await db.SaveChangesAsync(ct);
    }
}
