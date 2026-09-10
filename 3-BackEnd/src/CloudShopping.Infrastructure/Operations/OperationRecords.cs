namespace CloudShopping.Infrastructure.Operations;

public sealed class OperationEvent
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public int OrderId {get;set;}
    public string OperationKey {get;set;}="";
    public string RequestHash {get;set;}="";
    public string Kind {get;set;}="";
    public string PreviousState {get;set;}="";
    public string NewState {get;set;}="";
    public string Actor {get;set;}="";
    public string Notes {get;set;}="";
    public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class Shipment
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public int OrderId {get;set;}
    public string Carrier {get;set;}="";
    public string Service {get;set;}="";
    public string TrackingCode {get;set;}="";
    public int Volumes {get;set;}
    public string State {get;set;}="Dispatched";
    public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class ShipmentItem
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public string ShipmentId {get;set;}="";
    public int OrderItemId {get;set;}
    public int Quantity {get;set;}
}
public sealed class ReturnCase
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public int OrderId {get;set;}
    public int CustomerId {get;set;}
    public string Reason {get;set;}="";
    public string State {get;set;}="Requested";
    public string Decision {get;set;}="";
    public DateTime CreatedAt {get;set;}=DateTime.UtcNow;
}
public sealed class ReturnItem
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public string ReturnCaseId {get;set;}="";
    public int OrderItemId {get;set;}
    public int Quantity {get;set;}
    public int RestockedQuantity {get;set;}
}
