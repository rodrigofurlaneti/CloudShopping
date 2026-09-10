namespace CloudShopping.Infrastructure.Operations;
public sealed class WishlistItem
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public int CustomerId {get;set;}
 public int ProductId {get;set;}
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class ProductReview
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public int CustomerId {get;set;}
 public int ProductId {get;set;}
 public int OrderId {get;set;}
 public int Rating {get;set;}
 public string Content {get;set;}="";
 public string State {get;set;}="Pending";
 public string ModerationReason {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
 public int Version {get;set;}=1;
}
public sealed class ReviewDecision
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string ReviewId {get;set;}="";
 public string State {get;set;}="";
 public string Reason {get;set;}="";
 public string Actor {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class SupportTicket
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public int CustomerId {get;set;}
 public int? OrderId {get;set;}
 public string Subject {get;set;}="";
 public string Category {get;set;}="Question";
 public string State {get;set;}="Open";
 public int Version {get;set;}=1;
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class SupportMessage
{
 public string Id {get;set;}=Guid.NewGuid().ToString("N");
 public int TenantId {get;set;}
 public string TicketId {get;set;}="";
 public string Content {get;set;}="";
 public string Sender {get;set;}="";
 public string OperationKey {get;set;}="";
 public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
