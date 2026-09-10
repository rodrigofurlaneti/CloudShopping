using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Security;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class AccountSecurityRepository(AppDbContext db) : IAccountSecurityRepository
{
    private IQueryable<AuthSession> Owned(int subject,string kind) => db.Set<AuthSession>().Where(x=>x.TenantId==db.CurrentTenantId&&x.SubjectId==subject&&x.Kind==kind);
    public Task<AuthSession?> GetByIdAsync(string id,CancellationToken ct=default) => db.Set<AuthSession>().SingleOrDefaultAsync(x=>x.TenantId==db.CurrentTenantId&&x.Id==id,ct);
    public async Task AddAsync(AuthSession entity,CancellationToken ct=default) => await db.AddAsync(entity,ct);
    public void Update(AuthSession entity) => db.Update(entity);
    public void Remove(AuthSession entity) => entity.Revoke();
    public Task<AuthSession?> GetOwnedAsync(int subject,string kind,string id,CancellationToken ct) => Owned(subject,kind).SingleOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<AuthSession>> GetOwnedSessionsAsync(int subject,string kind,bool activeOnly,CancellationToken ct) =>
        await Owned(subject,kind).Where(x=>!activeOnly||x.RevokedAt==null).OrderByDescending(x=>x.ExpiresAt).ToListAsync(ct);
    public Task<EmployeeUser?> GetEmployeeAsync(int id,CancellationToken ct) => db.Set<EmployeeUser>().SingleOrDefaultAsync(x=>x.Id==id&&x.IsActive,ct);
    public Task<Customer?> GetCustomerAsync(int id,CancellationToken ct) => db.Customers.SingleOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<ISessionTransaction> BeginEditAsync(int subject,string kind,CancellationToken ct)
    {
        var gate=await PaymentLock.Acquire(db.Database.GetConnectionString()!, $"account:{db.CurrentTenantId}:{kind}:{subject}",ct);
        try { db.ChangeTracker.Clear(); return new Edit(gate,await db.Database.BeginTransactionAsync(ct)); }
        catch { await gate.DisposeAsync(); throw; }
    }
    private sealed class Edit(PaymentLock gate,IDbContextTransaction transaction) : ISessionTransaction
    {
        public Task Commit(CancellationToken ct) => transaction.CommitAsync(ct);
        public async ValueTask DisposeAsync() { try { await transaction.DisposeAsync(); } finally { await gate.DisposeAsync(); } }
    }
}
