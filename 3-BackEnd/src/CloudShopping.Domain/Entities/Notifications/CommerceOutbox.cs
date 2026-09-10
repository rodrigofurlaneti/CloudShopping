using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Entities.Orders;
namespace CloudShopping.Domain.Entities.Notifications;
public sealed class CommerceOutbox : Entity<string>
{
 private CommerceOutbox() { Id=Guid.NewGuid().ToString("N"); }
 public static CommerceOutbox Create(Order order,string kind)
 {
  ArgumentNullException.ThrowIfNull(order);
  if(string.IsNullOrWhiteSpace(kind))throw new ArgumentException("Tipo de evento obrigatório.");
  return new CommerceOutbox {TenantId=order.TenantId,Order=order,OrderVersion=order.Version,Kind=kind};
 }
 public void MarkProcessed() { State="Processed";ProcessedAt=DateTime.UtcNow;LastError=null; }
 public void Retry() { if(State!="Failed")return; State="Pending";AvailableAt=DateTime.UtcNow;Attempts=0; }


 public int TenantId {get;private set;}
 public int OrderId {get;private set;}
 public Order? Order {get;private set;}
 public int OrderVersion {get;private set;}
 public string Kind {get;private set;}="";
 public DateTime CreatedAt {get;private set;}=DateTime.UtcNow;
 public DateTime AvailableAt {get;private set;}=DateTime.UtcNow;
 public DateTime? ProcessedAt {get;private set;}
 public int Attempts {get;private set;}
 public string State {get;private set;}="Pending";
 public string? LastError {get;private set;}
}
