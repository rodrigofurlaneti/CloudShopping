using System.Security.Cryptography;
using System.Text;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudShopping.Infrastructure.Services;

public sealed record SessionSummary(string Id, DateTime ExpiresAt, bool Current);

public sealed class AccountSecurity(AppDbContext db, IPasswordHasher hasher)
{
    public static string Stamp(string credential) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));
    private IQueryable<AuthSession> Owned(int subject, string kind) => db.Set<AuthSession>()
        .Where(x => x.TenantId == db.CurrentTenantId && x.SubjectId == subject && x.Kind == kind);

    public async Task<List<SessionSummary>> Sessions(int subject, string kind, string current, CancellationToken ct)
    {
        await RequireCurrent(subject, kind, current, ct);
        return await Owned(subject, kind).Where(x => x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(x => x.ExpiresAt).Take(100)
            .Select(x => new SessionSummary(x.Id, x.ExpiresAt, x.Id == current)).ToListAsync(ct);
    }

    private async Task<AuthSession> RequireCurrent(int subject, string kind, string current, CancellationToken ct)
    {
        var session = await Owned(subject, kind).SingleOrDefaultAsync(x => x.Id == current, ct);
        if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Sessão expirada. Entre novamente.");
        string? credential = kind == "Administrator"
            ? (await db.Set<EmployeeUser>().SingleOrDefaultAsync(x => x.Id == subject && x.IsActive, ct))?.PasswordHash
            : (await db.Customers.SingleOrDefaultAsync(x => x.Id == subject, ct))?.PasswordHash;
        if (credential == null || session.CredentialStamp != Stamp(credential))
            throw new UnauthorizedAccessException("Entre com uma conta cadastrada para gerenciar o acesso.");
        return session;
    }

    public async Task Revoke(int subject, string kind, string current, string? target, CancellationToken ct)
    {
        await using var gate = await PaymentLock.Acquire(db.Database.GetConnectionString()!, $"account:{db.CurrentTenantId}:{kind}:{subject}", ct);
        db.ChangeTracker.Clear();
        await RequireCurrent(subject, kind, current, ct);
        if (target != null)
        {
            var session = await Owned(subject, kind).SingleOrDefaultAsync(x => x.Id == target, ct)
                ?? throw new KeyNotFoundException("Sessão não encontrada.");
            session.RevokedAt ??= DateTime.UtcNow;
        }
        else
        {
            var sessions = await Owned(subject, kind).Where(x => x.Id != current && x.RevokedAt == null).ToListAsync(ct);
            foreach (var session in sessions) session.RevokedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task ChangePassword(int subject, string kind, string current, string oldPassword, string newPassword, CancellationToken ct)
    {
        if (newPassword.Length < 12 || Encoding.UTF8.GetByteCount(newPassword) > 72)
            throw new ArgumentException("A nova senha deve ter pelo menos 12 caracteres e no máximo 72 bytes.");
        if (newPassword == oldPassword) throw new ArgumentException("Escolha uma senha diferente da atual.");
        await using var gate = await PaymentLock.Acquire(db.Database.GetConnectionString()!, $"account:{db.CurrentTenantId}:{kind}:{subject}", ct);
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await RequireCurrent(subject, kind, current, ct);
        if (kind == "Administrator")
        {
            var user = await db.Set<EmployeeUser>().SingleAsync(x => x.Id == subject, ct);
            if (!hasher.Verify(oldPassword, user.PasswordHash)) throw new ArgumentException("Senha atual incorreta.");
            user.UpdatePassword(hasher.Hash(newPassword));
        }
        else
        {
            var customer = await db.Customers.SingleAsync(x => x.Id == subject, ct);
            if (!hasher.Verify(oldPassword, customer.PasswordHash!)) throw new ArgumentException("Senha atual incorreta.");
            customer.SetPassword(hasher.Hash(newPassword));
        }
        foreach (var session in await Owned(subject, kind).Where(x => x.RevokedAt == null).ToListAsync(ct))
            session.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
