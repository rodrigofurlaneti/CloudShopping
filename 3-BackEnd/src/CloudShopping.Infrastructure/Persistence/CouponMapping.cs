using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
 private void ConfigureCoupons(ModelBuilder b)
 {
  b.Entity<Coupon>().ToTable("coupons").HasKey(x=>x.Id);
  b.Entity<Coupon>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<Coupon>().Property(x=>x.Version).IsConcurrencyToken();
  b.Entity<Coupon>().Property(x=>x.Value).HasColumnType("decimal(12,2)");
  b.Entity<Coupon>().Property(x=>x.MinimumSubtotal).HasColumnType("decimal(12,2)");
  b.Entity<CouponRedemption>().ToTable("couponredemptions").HasKey(x=>x.Id);
  b.Entity<CouponRedemption>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<CouponRedemption>().Property(x=>x.DiscountAmount).HasColumnType("decimal(12,2)");
  b.Entity<CouponRedemption>().HasOne(x=>x.Order).WithMany().HasForeignKey(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<CloudShopping.Domain.Entities.Orders.Order>().Property(x=>x.DiscountAmount).HasColumnType("decimal(12,2)");
 }
}
