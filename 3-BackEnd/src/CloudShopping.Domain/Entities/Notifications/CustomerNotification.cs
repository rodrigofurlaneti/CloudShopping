using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Entities.Orders;
namespace CloudShopping.Domain.Entities.Notifications;
public sealed class CustomerNotification : Entity<string>
{
 private CustomerNotification() { Id=Guid.NewGuid().ToString("N"); }
 public static CustomerNotification Create(int tenantId,int customerId,int orderId,string eventId,string kind)
 {
  if(tenantId<=0||customerId<=0||orderId<=0||string.IsNullOrWhiteSpace(eventId)||string.IsNullOrWhiteSpace(kind))throw new ArgumentException("Notificação inválida.");
  return new CustomerNotification {TenantId=tenantId,CustomerId=customerId,OrderId=orderId,EventId=eventId,Kind=kind};
 }
 public void MarkRead() => ReadAt ??= DateTime.UtcNow;


 public int TenantId {get;private set;}
 public int CustomerId {get;private set;}
 public int OrderId {get;private set;}
 public string EventId {get;private set;}="";
 public string Kind {get;private set;}="";
 public DateTime CreatedAt {get;private set;}=DateTime.UtcNow;
 public DateTime? ReadAt {get;private set;}
}
