using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Entities.Orders;
using System.Text.RegularExpressions;
namespace CloudShopping.Domain.Entities.Promotions;
public sealed class CouponRedemption : Entity<string>
{
 private CouponRedemption() { Id=Guid.NewGuid().ToString("N"); }
 public static CouponRedemption Create(int tenantId,string couponId,Order order,int customerId,decimal discount)
 {
  if(tenantId<=0||string.IsNullOrWhiteSpace(couponId)||customerId<=0||discount<0)throw new ArgumentException("Utilização de cupom inválida.");
  ArgumentNullException.ThrowIfNull(order);
  return new CouponRedemption {TenantId=tenantId,CouponId=couponId,Order=order,CustomerId=customerId,DiscountAmount=discount};
 }
 public void Release() => Released=true;

 public int TenantId {get;private set;}
 public string CouponId {get;private set;}="";
 public int OrderId {get;private set;}
 public Order? Order {get;private set;}
 public int CustomerId {get;private set;}
 public decimal DiscountAmount {get;private set;}
 public bool Released {get;private set;}
}
