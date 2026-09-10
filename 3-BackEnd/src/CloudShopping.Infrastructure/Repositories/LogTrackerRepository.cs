using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;

public sealed class LogTrackerRepository(AppDbContext db, ITenantProvider tenant) : ILogTrackerRepository
{
    private int Tenant => tenant.GetTenantId() is > 0 and var id ? id : throw new UnauthorizedAccessException("Empresa não identificada.");
    private IQueryable<LogTracker> Scoped => db.Set<LogTracker>().Where(x => x.TenantId == Tenant);
    public Task<LogTracker?> GetByIdAsync(long id, CancellationToken cancellationToken = default) => Scoped.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task AddAsync(LogTracker entity, CancellationToken cancellationToken = default)
    { Check(entity); await db.Set<LogTracker>().AddAsync(entity, cancellationToken); }
    public void Update(LogTracker entity) { Check(entity); db.Set<LogTracker>().Update(entity); }
    public void Remove(LogTracker entity) { Check(entity); db.Set<LogTracker>().Remove(entity); }
    private void Check(LogTracker entity)
    { if(entity.TenantId != Tenant) throw new UnauthorizedAccessException("Registro de outra empresa."); }
    public async Task<(IReadOnlyList<LogTracker> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, string? outcome, CancellationToken cancellationToken = default)
    {
        if(page < 1 || page > 100000 || pageSize < 1 || pageSize > 200) throw new ArgumentException("Paginação inválida.");
        var query = Scoped.AsNoTracking();
        if(outcome != null) query = query.Where(x => x.Outcome == outcome);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, count);
    }
}
