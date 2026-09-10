using CloudShopping.Domain.Entities.Promotions;
namespace CloudShopping.Application.Abstractions.Data;
public interface ICouponRepository : IRepository<Coupon, string>
{
    Task<IReadOnlyList<Coupon>> GetPageAsync(int page, int pageSize, CancellationToken ct);
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}
