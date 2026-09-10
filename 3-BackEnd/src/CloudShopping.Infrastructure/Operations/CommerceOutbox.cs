using CloudShopping.Domain.Entities.Orders;
namespace CloudShopping.Infrastructure.Operations;
public sealed class CommerceOutbox
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public int OrderId {get;set;}
 public Order? Order {get;set;}
 public int OrderVersion {get;set;}
 public string Kind {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
 public DateTime AvailableAt {get;set;}=DateTime.UtcNow;
 public DateTime? ProcessedAt {get;set;}
 public int Attempts {get;set;}
 public string State {get;set;}="Pending";
 public string? LastError {get;set;}
}
public sealed class CustomerNotification
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public int CustomerId {get;set;}
 public int OrderId {get;set;}
 public string EventId {get;set;}="";
 public string Kind {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
 public DateTime? ReadAt {get;set;}
}
