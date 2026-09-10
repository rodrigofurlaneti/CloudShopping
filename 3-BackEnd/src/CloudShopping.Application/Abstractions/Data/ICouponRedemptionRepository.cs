using CloudShopping.Domain.Entities.Promotions;
namespace CloudShopping.Application.Abstractions.Data;
public interface ICouponRedemptionRepository
{
    Task<int> CountActiveAsync(string couponId, int customerId, CancellationToken ct);
    Task<CouponRedemption?> GetActiveByOrderAsync(int orderId, CancellationToken ct);
    void Add(CouponRedemption redemption);
}
