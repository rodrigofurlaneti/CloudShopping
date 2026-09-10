using CloudShopping.Domain.Entities.Security;
using System.Security.Cryptography;
using System.Text;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Application.Abstractions.Data;



namespace CloudShopping.Application.Features.AccountSecurity;

public sealed record SessionSummary(string Id, DateTime ExpiresAt, bool Current);

public sealed class AccountSecurity(IAccountSecurityRepository repository, IUnitOfWork unitOfWork, IPasswordHasher hasher)
{
    public static string Stamp(string credential) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential)));
    public async Task<List<SessionSummary>> Sessions(int subject, string kind, string current, CancellationToken ct)
    {
        await RequireCurrent(subject, kind, current, ct);
        return (await repository.GetOwnedSessionsAsync(subject,kind,true,ct)).Where(x=>x.ExpiresAt>DateTime.UtcNow).Take(100)
            .Select(x=>new SessionSummary(x.Id,x.ExpiresAt,x.Id==current)).ToList();
    }

    private async Task<AuthSession> RequireCurrent(int subject, string kind, string current, CancellationToken ct)
    {
        var session = await repository.GetOwnedAsync(subject,kind,current,ct);
        if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Sessão expirada. Entre novamente.");
        string? credential = kind == "Administrator"
            ? (await repository.GetEmployeeAsync(subject,ct))?.PasswordHash
            : (await repository.GetCustomerAsync(subject,ct))?.PasswordHash;
        if (credential == null || session.CredentialStamp != Stamp(credential))
            throw new UnauthorizedAccessException("Entre com uma conta cadastrada para gerenciar o acesso.");
        return session;
    }

    public async Task Revoke(int subject, string kind, string current, string? target, CancellationToken ct)
    {
        await using var transaction = await repository.BeginEditAsync(subject,kind,ct);
        await RequireCurrent(subject, kind, current, ct);
        if (target != null)
        {
            var session = await repository.GetOwnedAsync(subject,kind,target,ct)
                ?? throw new KeyNotFoundException("Sessão não encontrada.");
            session.Revoke();
        }
        else
        {
            var sessions = (await repository.GetOwnedSessionsAsync(subject,kind,true,ct)).Where(x=>x.Id!=current);
            foreach (var session in sessions) session.Revoke();
        }
        await unitOfWork.CommitAsync(ct);
        await transaction.Commit(ct);
    }

    public async Task ChangePassword(int subject, string kind, string current, string oldPassword, string newPassword, CancellationToken ct)
    {
        if (newPassword.Length < 12 || Encoding.UTF8.GetByteCount(newPassword) > 72)
            throw new ArgumentException("A nova senha deve ter pelo menos 12 caracteres e no máximo 72 bytes.");
        if (newPassword == oldPassword) throw new ArgumentException("Escolha uma senha diferente da atual.");
        await using var transaction = await repository.BeginEditAsync(subject,kind,ct);
        await RequireCurrent(subject, kind, current, ct);
        if (kind == "Administrator")
        {
            var user = await repository.GetEmployeeAsync(subject,ct) ?? throw new UnauthorizedAccessException();
            if (!hasher.Verify(oldPassword, user.PasswordHash)) throw new ArgumentException("Senha atual incorreta.");
            user.UpdatePassword(hasher.Hash(newPassword));
        }
        else
        {
            var customer = await repository.GetCustomerAsync(subject,ct) ?? throw new UnauthorizedAccessException();
            if (!hasher.Verify(oldPassword, customer.PasswordHash!)) throw new ArgumentException("Senha atual incorreta.");
            customer.SetPassword(hasher.Hash(newPassword));
        }
        foreach (var session in await repository.GetOwnedSessionsAsync(subject,kind,true,ct))
            session.Revoke();
        await unitOfWork.CommitAsync(ct);
        await transaction.Commit(ct);
    }
}
