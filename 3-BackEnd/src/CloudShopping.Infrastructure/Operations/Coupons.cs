using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using CloudShopping.Infrastructure.Payments;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace CloudShopping.Infrastructure.Operations;
public sealed class Coupon
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string Code {get;set;}="";
 public string Kind {get;set;}="";
 public decimal Value {get;set;}
 public decimal MinimumSubtotal {get;set;}
 public int UsageLimit {get;set;}
 public int PerCustomerLimit {get;set;}
 public int UsedCount {get;set;}
 public DateTime StartsAt {get;set;}
 public DateTime EndsAt {get;set;}
 public bool Enabled {get;set;}=true;
 public int Version {get;set;}=1;
}
public sealed class CouponRedemption
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string CouponId {get;set;}="";
 public int OrderId {get;set;}
 public Order? Order {get;set;}
 public int CustomerId {get;set;}
 public decimal DiscountAmount {get;set;}
 public bool Released {get;set;}
}
public sealed record CouponInput(string Code,string Kind,decimal Value,decimal MinimumSubtotal,int UsageLimit,int PerCustomerLimit,DateTime StartsAt,DateTime EndsAt);
public sealed record CouponQuote(Coupon? Coupon,decimal Discount,string Fingerprint);
public sealed class Coupons(AppDbContext db)
{
 public static string? Normalize(string? code)
 {if(string.IsNullOrWhiteSpace(code))return null;var normalized=code.Trim().ToUpperInvariant();if(!Regex.IsMatch(normalized,"^[A-Z0-9_-]{3,40}$"))throw new ArgumentException("Código de cupom inválido.");return normalized;}
 public async Task<CouponQuote> Evaluate(int customer,decimal subtotal,string? code,CancellationToken ct)
 {
  code=Normalize(code);if(code==null)return new(null,0,"");
  var c=await db.Set<Coupon>().SingleOrDefaultAsync(x=>x.Code==code,ct);
  var now=DateTime.UtcNow;
  if(c==null||!c.Enabled||now<c.StartsAt||now>=c.EndsAt||subtotal<c.MinimumSubtotal||c.UsedCount>=c.UsageLimit)
   throw new CommerceConflictException("Cupom indisponível, expirado ou subtotal insuficiente.");
  if(await db.Set<CouponRedemption>().CountAsync(x=>x.CouponId==c.Id&&x.CustomerId==customer&&!x.Released,ct)>=c.PerCustomerLimit)throw new CommerceConflictException("Limite de uso deste cupom por cliente atingido.");
  var discount=Math.Min(subtotal,decimal.Round(c.Kind=="Percent"?subtotal*c.Value/100:c.Value,2,MidpointRounding.AwayFromZero));
  var culture=System.Globalization.CultureInfo.InvariantCulture;
  var fingerprint=JsonFields.Hash(JsonSerializer.Serialize(new{c.Id,c.Code,c.Kind,value=c.Value.ToString("F2",culture),minimum=c.MinimumSubtotal.ToString("F2",culture),c.UsageLimit,c.PerCustomerLimit,start=c.StartsAt.ToString("yyyyMMddHHmmssffffff",culture),end=c.EndsAt.ToString("yyyyMMddHHmmssffffff",culture),c.Enabled}));
  return new(c,discount,fingerprint);
 }
 public void Redeem(Order order,int customer,CouponQuote quote)
 {
  if(quote.Coupon==null)return;quote.Coupon.UsedCount++;
  db.Add(new CouponRedemption {TenantId=db.CurrentTenantId,CouponId=quote.Coupon.Id,Order=order,CustomerId=customer,DiscountAmount=quote.Discount});
 }
 public async Task Release(int orderId,CancellationToken ct)
 {
  var r=await db.Set<CouponRedemption>().SingleOrDefaultAsync(x=>x.OrderId==orderId&&!x.Released,ct);if(r==null)return;
  var c=await db.Set<Coupon>().SingleAsync(x=>x.Id==r.CouponId,ct);c.UsedCount--;r.Released=true;
 }
 public async Task<Coupon> Create(CouponInput input,CancellationToken ct)
 {
  var code=Normalize(input.Code)??throw new ArgumentException("Código obrigatório.");
  if(input.Kind is not("Fixed" or "Percent")||input.Value<=0||input.Value>100000||decimal.Round(input.Value,2)!=input.Value||(input.Kind=="Percent"&&input.Value>100)||input.MinimumSubtotal<0||input.MinimumSubtotal>1000000||decimal.Round(input.MinimumSubtotal,2)!=input.MinimumSubtotal||input.UsageLimit is <1 or >1000000||input.PerCustomerLimit<1||input.PerCustomerLimit>input.UsageLimit||input.EndsAt<=input.StartsAt||input.EndsAt<=DateTime.UtcNow)
   throw new ArgumentException("Regras de cupom inválidas. Confira valor, limites e vigência UTC.");
  if(await db.Set<Coupon>().AnyAsync(x=>x.Code==code,ct))throw new CommerceConflictException("Código já cadastrado.");
  var c=new Coupon {TenantId=db.CurrentTenantId,Code=code,Kind=input.Kind,Value=input.Value,MinimumSubtotal=input.MinimumSubtotal,UsageLimit=input.UsageLimit,PerCustomerLimit=input.PerCustomerLimit,StartsAt=input.StartsAt.ToUniversalTime(),EndsAt=input.EndsAt.ToUniversalTime()};
  db.Add(c);await db.SaveChangesAsync(ct);return c;
 }
}
