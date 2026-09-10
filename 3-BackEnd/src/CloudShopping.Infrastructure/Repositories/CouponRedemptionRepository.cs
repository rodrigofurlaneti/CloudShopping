using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class CouponRedemptionRepository(AppDbContext db) : ICouponRedemptionRepository
{
    public Task<int> CountActiveAsync(string couponId, int customerId, CancellationToken ct) =>
        db.Set<CouponRedemption>().CountAsync(x => x.CouponId == couponId && x.CustomerId == customerId && !x.Released, ct);
    public Task<CouponRedemption?> GetActiveByOrderAsync(int orderId, CancellationToken ct) =>
        db.Set<CouponRedemption>().SingleOrDefaultAsync(x => x.OrderId == orderId && !x.Released, ct);
    public void Add(CouponRedemption redemption) => db.Add(redemption);
}
