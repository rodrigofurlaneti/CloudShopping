using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Entities.Orders;
using System.Text.RegularExpressions;
namespace CloudShopping.Domain.Entities.Promotions;
public sealed class Coupon : Entity<string>
{
 private Coupon() { Id=Guid.NewGuid().ToString("N"); }
 public static Coupon Create(int tenantId,string code,string kind,decimal value,decimal minimumSubtotal,int usageLimit,int perCustomerLimit,DateTime startsAt,DateTime endsAt)
 {
  code=Normalize(code)??throw new ArgumentException("Código obrigatório.");
  if(tenantId<=0||kind is not("Fixed" or "Percent")||value<=0||value>100000||decimal.Round(value,2)!=value||(kind=="Percent"&&value>100)||minimumSubtotal<0||minimumSubtotal>1000000||decimal.Round(minimumSubtotal,2)!=minimumSubtotal||usageLimit is <1 or >1000000||perCustomerLimit<1||perCustomerLimit>usageLimit||endsAt<=startsAt||endsAt<=DateTime.UtcNow) throw new ArgumentException("Regras de cupom inválidas. Confira valor, limites e vigência UTC.");
  return new Coupon {TenantId=tenantId,Code=code,Kind=kind,Value=value,MinimumSubtotal=minimumSubtotal,UsageLimit=usageLimit,PerCustomerLimit=perCustomerLimit,StartsAt=startsAt.ToUniversalTime(),EndsAt=endsAt.ToUniversalTime()};
 }
 public static string? Normalize(string? code)
 {if(string.IsNullOrWhiteSpace(code))return null;var normalized=code.Trim().ToUpperInvariant();if(!Regex.IsMatch(normalized,"^[A-Z0-9_-]{3,40}$"))throw new ArgumentException("Código de cupom inválido.");return normalized;}
 public void SetEnabled(bool enabled) => Enabled=enabled;
 public void Redeem() { if(UsedCount>=UsageLimit)throw new InvalidOperationException("Limite de uso do cupom atingido."); UsedCount++; }
 public void Release() { if(UsedCount<=0)throw new InvalidOperationException("Cupom sem utilização para liberar."); UsedCount--; }

 public int TenantId {get;private set;}
 public string Code {get;private set;}="";
 public string Kind {get;private set;}="";
 public decimal Value {get;private set;}
 public decimal MinimumSubtotal {get;private set;}
 public int UsageLimit {get;private set;}
 public int PerCustomerLimit {get;private set;}
 public int UsedCount {get;private set;}
 public DateTime StartsAt {get;private set;}
 public DateTime EndsAt {get;private set;}
 public bool Enabled {get;private set;}=true;
 public int Version {get;private set;}=1;
}
