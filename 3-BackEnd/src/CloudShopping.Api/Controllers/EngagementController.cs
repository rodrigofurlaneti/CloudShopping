using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/store"),Authorize(Roles="Customer")]
public sealed class EngagementController(CustomerEngagement service):ControllerBase
{
 private int Customer=>StoreSecurity.Subject(User);
 [HttpGet("favorites")]public Task<object> Favorites(int page=1,CancellationToken ct=default)=>service.Favorites(Customer,page,ct);
 [HttpPut("favorites/{id:int}")]public async Task<IActionResult> Favorite(int id,CancellationToken ct){await service.Favorite(Customer,id,true,ct);return NoContent();}
 [HttpDelete("favorites/{id:int}")]public async Task<IActionResult> Remove(int id,CancellationToken ct){await service.Favorite(Customer,id,false,ct);return NoContent();}
 [HttpGet("products/{id:int}/reviews"),AllowAnonymous]public Task<object> Reviews(int id,int page=1,CancellationToken ct=default)=>service.Reviews(id,page,ct);
 [HttpPost("products/{id:int}/reviews")]public Task<object> Review(int id,ReviewInput input,CancellationToken ct)=>service.Review(Customer,id,input,ct);
 [HttpGet("support")]public Task<object> Tickets(int page=1,CancellationToken ct=default)=>service.Tickets(Customer,page,ct);
 [HttpGet("support/{id}")]public Task<object> Ticket(string id,CancellationToken ct)=>service.Ticket(id,Customer,ct);
 [HttpPost("support")]public Task<object> Open(TicketInput input,CancellationToken ct)=>service.OpenTicket(Customer,input,ct);
 [HttpPost("support/{id}/messages")]public Task<object> Reply(string id,MessageInput input,CancellationToken ct)=>service.Reply(id,Customer,input,ct);
}
[ApiController,Route("api/v1/engagement"),Authorize(Roles="Administrator")]
public sealed class EngagementAdminController(CustomerEngagement service):ControllerBase
{
 [HttpGet("reviews")]public Task<object> Reviews(int page=1,string state="Pending",CancellationToken ct=default)=>service.Moderation(page,state,ct);
 [HttpPost("reviews/{id}")]public async Task<IActionResult> Moderate(string id,ReviewDecisionInput input,CancellationToken ct){await service.Moderate(id,input,"Administrator:"+StoreSecurity.Subject(User),ct);return NoContent();}
 [HttpGet("support")]public Task<object> Tickets(int page=1,CancellationToken ct=default)=>service.Tickets(null,page,ct);
 [HttpGet("support/{id}")]public Task<object> Ticket(string id,CancellationToken ct)=>service.Ticket(id,null,ct);
 [HttpPost("support/{id}/messages")]public Task<object> Reply(string id,MessageInput input,CancellationToken ct)=>service.Reply(id,null,input,ct);
}
