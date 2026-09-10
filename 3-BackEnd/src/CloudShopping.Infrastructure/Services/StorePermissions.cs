using System.Text.Json;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudShopping.Infrastructure.Services;

public sealed class ProfilePermission
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ProfileId { get; set; }
    public string Permission { get; set; } = "";
}
public sealed class AccessChange
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int TenantId { get; set; }
    public int ProfileId { get; set; }
    public int ActorId { get; set; }
    public string BeforeJson { get; set; } = "[]";
    public string AfterJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public sealed class StorePermissions(AppDbContext db)
{
    public static readonly string[] Modules = ["catalog", "orders", "customers", "finance", "settings", "support", "moderation", "promotions", "reports"];
    public static readonly string[] Available = Modules.SelectMany(x => new[] { x + ".read", x + ".write" }).Append("stock.write").Append("finance.refund").ToArray();
    public async Task<string[]> ForUser(int userId, CancellationToken ct = default)
    {
        var profiles = from membership in db.Set<ProfileUser>() join profile in db.Set<Profile>() on membership.ProfileId equals profile.Id
            where membership.EmployeeUserId == userId && membership.IsActive && profile.IsActive select profile;
        if (await profiles.AnyAsync(x => x.Name == "Administrador Geral", ct)) return ["*"];
        return await db.Set<ProfilePermission>().Where(x => profiles.Any(p => p.Id == x.ProfileId))
            .Select(x => x.Permission).Distinct().ToArrayAsync(ct);
    }
    public static bool Allows(IEnumerable<string> permissions, string? required) => permissions.Contains("*") || required != null && permissions.Contains(required);

    public async Task Replace(int actorId, int profileId, string[] expected, string[] desired, CancellationToken ct)
    {
        if (desired.Length > Available.Length || expected.Length > Available.Length || desired.Any(x => !Available.Contains(x)))
            throw new ArgumentException("Permissões inválidas.");
        var normalized = desired.Distinct().Order().ToArray();
        if (normalized.Any(x => x.EndsWith(".write") && x != "stock.write" && !normalized.Contains(x.Replace(".write", ".read"))))
            throw new ArgumentException("Inclua a leitura do módulo antes de habilitar alterações.");
        if (normalized.Contains("stock.write") && !normalized.Contains("catalog.read") || normalized.Contains("finance.refund") && !normalized.Contains("finance.read"))
            throw new ArgumentException("Inclua a leitura de catálogo/financeiro para estoque/estorno.");
        await using var gate = await PaymentLock.Acquire(db.Database.GetConnectionString()!, $"profile:{db.CurrentTenantId}:{profileId}", ct);
        db.ChangeTracker.Clear();
        if (!(await ForUser(actorId, ct)).Contains("*")) throw new UnauthorizedAccessException();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var profile = await db.Set<Profile>().SingleOrDefaultAsync(x => x.Id == profileId, ct) ?? throw new KeyNotFoundException();
        if (profile.Name == "Administrador Geral") throw new InvalidOperationException("O perfil administrador geral possui acesso integral e não aceita permissões parciais.");
        var rows = await db.Set<ProfilePermission>().Where(x => x.ProfileId == profileId).ToListAsync(ct);
        var before = rows.Select(x => x.Permission).Order().ToArray();
        if (!before.SequenceEqual(expected.Distinct().Order())) throw new InvalidOperationException("Permissões alteradas por outro administrador. Atualize a tela.");
        db.RemoveRange(rows.Where(x => !normalized.Contains(x.Permission)));
        foreach (var permission in normalized.Except(before)) db.Add(new ProfilePermission { TenantId = db.CurrentTenantId, ProfileId = profileId, Permission = permission });
        if (!before.SequenceEqual(normalized)) db.Add(new AccessChange { TenantId = db.CurrentTenantId, ProfileId = profileId, ActorId = actorId, BeforeJson = JsonSerializer.Serialize(before), AfterJson = JsonSerializer.Serialize(normalized) });
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
    }
}
