using CloudShopping.Domain.Entities.Promotions;
namespace CloudShopping.Application.Features.Coupons.ViewModels;
public sealed record CouponView(string Id, int TenantId, string Code, string Kind, decimal Value,
    decimal MinimumSubtotal, int UsageLimit, int PerCustomerLimit, int UsedCount,
    DateTime StartsAt, DateTime EndsAt, bool Enabled, int Version)
{
    public static CouponView From(Coupon c) => new(c.Id, c.TenantId, c.Code, c.Kind, c.Value,
        c.MinimumSubtotal, c.UsageLimit, c.PerCustomerLimit, c.UsedCount, c.StartsAt, c.EndsAt, c.Enabled, c.Version);
}
