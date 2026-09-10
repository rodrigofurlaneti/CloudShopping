using CloudShopping.Domain.Exceptions;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using System.Security.Cryptography;
using System.Text;



using System.Text.Json;
using System.Text.RegularExpressions;
namespace CloudShopping.Application.Features.Coupons;
public sealed record CouponQuote(Coupon? Coupon,decimal Discount,string Fingerprint);
public sealed class CouponCheckoutService(ICouponRepository coupons, ICouponRedemptionRepository redemptions, ITenantProvider tenant)
{
 public static string? Normalize(string? code)
 {if(string.IsNullOrWhiteSpace(code))return null;var normalized=code.Trim().ToUpperInvariant();if(!Regex.IsMatch(normalized,"^[A-Z0-9_-]{3,40}$"))throw new ArgumentException("Código de cupom inválido.");return normalized;}
 public async Task<CouponQuote> Evaluate(int customer,decimal subtotal,string? code,CancellationToken ct)
 {
  code=Normalize(code);if(code==null)return new(null,0,"");
  var c=await coupons.GetByCodeAsync(code,ct);
  var now=DateTime.UtcNow;
  if(c==null||!c.Enabled||now<c.StartsAt||now>=c.EndsAt||subtotal<c.MinimumSubtotal||c.UsedCount>=c.UsageLimit)
   throw new CommerceConflictException("Cupom indisponível, expirado ou subtotal insuficiente.");
  if(await redemptions.CountActiveAsync(c.Id,customer,ct)>=c.PerCustomerLimit)throw new CommerceConflictException("Limite de uso deste cupom por cliente atingido.");
  var discount=Math.Min(subtotal,decimal.Round(c.Kind=="Percent"?subtotal*c.Value/100:c.Value,2,MidpointRounding.AwayFromZero));
  var culture=System.Globalization.CultureInfo.InvariantCulture;
  var fingerprint=Hash(JsonSerializer.Serialize(new{c.Id,c.Code,c.Kind,value=c.Value.ToString("F2",culture),minimum=c.MinimumSubtotal.ToString("F2",culture),c.UsageLimit,c.PerCustomerLimit,start=c.StartsAt.ToString("yyyyMMddHHmmssffffff",culture),end=c.EndsAt.ToString("yyyyMMddHHmmssffffff",culture),c.Enabled}));
  return new(c,discount,fingerprint);
 }
 public void Redeem(Order order,int customer,CouponQuote quote)
 {
  if(quote.Coupon==null)return;quote.Coupon.Redeem();
  redemptions.Add(CouponRedemption.Create(tenant.GetTenantId(),quote.Coupon.Id,order,customer,quote.Discount));
 }
 public async Task Release(int orderId,CancellationToken ct)
 {
  var r=await redemptions.GetActiveByOrderAsync(orderId,ct);if(r==null)return;
  var c=await coupons.GetByIdAsync(r.CouponId,ct) ?? throw new KeyNotFoundException("Cupom não encontrado.");c.Release();r.Release();
 }
 private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
