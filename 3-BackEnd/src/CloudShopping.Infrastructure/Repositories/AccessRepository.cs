using System.Text.Json;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace CloudShopping.Infrastructure.Repositories;

public sealed class AccessRepository(AppDbContext db) : IAccessRepository
{
    public async Task<IReadOnlyList<AccessProfile>> Profiles(CancellationToken ct)
    {
        var profiles = await db.Set<Profile>().AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var permissions = await db.Set<ProfilePermission>().AsNoTracking().ToListAsync(ct);
        return profiles.Select(x => new AccessProfile(x.Id, x.Name, x.IsActive,
            permissions.Where(p => p.ProfileId == x.Id).Select(p => p.Permission).Order().ToArray())).ToArray();
    }
    public async Task<AccessProfile?> Profile(int id, CancellationToken ct)
    {
        var profile = await db.Set<Profile>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (profile == null) return null;
        var permissions = await db.Set<ProfilePermission>().Where(x => x.ProfileId == id).Select(x => x.Permission).ToArrayAsync(ct);
        return new(profile.Id, profile.Name, profile.IsActive, permissions);
    }
    public async Task<string[]> ForUser(int userId, CancellationToken ct)
    {
        var profiles = from membership in db.Set<ProfileUser>() join profile in db.Set<Profile>() on membership.ProfileId equals profile.Id
            where membership.EmployeeUserId == userId && membership.IsActive && profile.IsActive select profile;
        if (await profiles.AnyAsync(x => x.Name == PermissionPolicy.GeneralAdministrator, ct)) return ["*"];
        return await db.Set<ProfilePermission>().Where(x => profiles.Any(p => p.Id == x.ProfileId)).Select(x => x.Permission).Distinct().ToArrayAsync(ct);
    }
    public async Task<IReadOnlyList<AccessChangeView>> Changes(int page, int pageSize, CancellationToken ct) => await db.Set<AccessChange>().AsNoTracking()
        .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
        .Select(x => new AccessChangeView(x.Id, x.TenantId, x.ProfileId, x.ActorId, x.BeforeJson, x.AfterJson, x.CreatedAt)).ToListAsync(ct);
    public async Task<IAccessEdit> BeginEdit(int profileId, CancellationToken ct)
    {
        var gate = await PaymentLock.Acquire(db.Database.GetConnectionString()!, $"profile:{db.CurrentTenantId}:{profileId}", ct);
        try
        {
            db.ChangeTracker.Clear();
            return new Edit(gate, await db.Database.BeginTransactionAsync(ct));
        }
        catch { await gate.DisposeAsync(); throw; }
    }
    public async Task SavePermissions(int actorId, int profileId, string[] before, string[] desired, CancellationToken ct)
    {
        var rows = await db.Set<ProfilePermission>().Where(x => x.ProfileId == profileId).ToListAsync(ct);
        db.RemoveRange(rows.Where(x => !desired.Contains(x.Permission)));
        foreach (var permission in desired.Except(before)) db.Add(ProfilePermission.Create(db.CurrentTenantId, profileId, permission));
        db.Add(AccessChange.Create(db.CurrentTenantId, profileId, actorId, before, desired));
        await db.SaveChangesAsync(ct);
    }
    private sealed class Edit(PaymentLock gate, IDbContextTransaction transaction) : IAccessEdit
    {
        public Task Commit(CancellationToken ct) => transaction.CommitAsync(ct);
        public async ValueTask DisposeAsync()
        {
            try { await transaction.DisposeAsync(); }
            finally { await gate.DisposeAsync(); }
        }
    }
}
