using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;

[ApiController,Route("api/v1/operations"),Authorize(Roles="Administrator")]
public sealed class OperationsController(OrderOperations operations):ControllerBase
{
    private string Actor=>"Administrator:"+StoreSecurity.Subject(User);
    [HttpGet("orders")]
    public Task<object> List(int page=1,string? state=null,int? orderId=null,CancellationToken ct=default)=>operations.List(page,state,orderId,ct);
    [HttpGet("orders/{id:int}")]
    public Task<object> Detail(int id,CancellationToken ct)=>operations.Detail(id,null,ct);
    [HttpPost("orders/{id:int}/transition")]
    public Task<object> Transition(int id,OperationInput input,CancellationToken ct)=>operations.Transition(id,input,Actor,ct);
    [HttpPost("orders/{id:int}/shipments")]
    public Task<object> Dispatch(int id,DispatchInput input,CancellationToken ct)=>operations.Dispatch(id,input,Actor,ct);
    [HttpPost("orders/{id:int}/shipments/{shipmentId}/tracking")]
    public Task<object> Tracking(int id,string shipmentId,TrackingInput input,CancellationToken ct)=>operations.Track(id,shipmentId,input,Actor,ct);
    [HttpPost("orders/{id:int}/returns/{returnId}")]
    public Task<object> Decide(int id,string returnId,ReturnDecisionInput input,CancellationToken ct)=>operations.DecideReturn(id,returnId,input,Actor,ct);
}
[ApiController,Route("api/v1/store/orders/{id:int}/fulfillment"),Authorize(Roles="Customer")]
public sealed class CustomerOperationsController(OrderOperations operations):ControllerBase
{
    [HttpGet]
    public Task<object> Detail(int id,CancellationToken ct)=>operations.Detail(id,StoreSecurity.Subject(User),ct);
    [HttpPost("returns")]
    public Task<object> RequestReturn(int id,ReturnInput input,CancellationToken ct)=>operations.RequestReturn(id,StoreSecurity.Subject(User),input,ct);
}
