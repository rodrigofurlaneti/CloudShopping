using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class CouponRepository(AppDbContext db) : ICouponRepository
{
    public Task<Coupon?> GetByIdAsync(string id, CancellationToken ct = default) => db.Set<Coupon>().SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(Coupon entity, CancellationToken ct = default) => await db.Set<Coupon>().AddAsync(entity, ct);
    public void Update(Coupon entity) => db.Set<Coupon>().Update(entity);
    public void Remove(Coupon entity) => entity.SetEnabled(false);
    public Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct) => db.Set<Coupon>().SingleOrDefaultAsync(x => x.Code == code, ct);
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) => db.Set<Coupon>().AnyAsync(x => x.Code == code, ct);
    public async Task<IReadOnlyList<Coupon>> GetPageAsync(int page, int pageSize, CancellationToken ct) =>
        await db.Set<Coupon>().AsNoTracking().OrderByDescending(x => x.StartsAt).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
}
