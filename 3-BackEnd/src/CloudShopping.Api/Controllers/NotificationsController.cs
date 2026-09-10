using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/store/notifications"),Authorize(Roles="Customer")]
public sealed class NotificationsController(AppDbContext db):ControllerBase
{
 [HttpGet]public async Task<object> List(int page=1,CancellationToken ct=default)=>await db.Set<CustomerNotification>().Where(x=>x.CustomerId==StoreSecurity.Subject(User)).OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).Select(x=>new{x.Id,x.OrderId,x.Kind,x.CreatedAt,x.ReadAt}).ToListAsync(ct);
 [HttpPost("{id}/read")]public async Task<IActionResult> Read(string id,CancellationToken ct)
 {var n=await db.Set<CustomerNotification>().SingleOrDefaultAsync(x=>x.Id==id&&x.CustomerId==StoreSecurity.Subject(User),ct)??throw new KeyNotFoundException();n.ReadAt??=DateTime.UtcNow;await db.SaveChangesAsync(ct);return NoContent();}
}
[ApiController,Route("api/v1/notifications"),Authorize(Roles="Administrator")]
public sealed class NotificationAdminController(AppDbContext db):ControllerBase
{
 [HttpGet]public async Task<object> List(int page=1,CancellationToken ct=default)=>await db.Set<CommerceOutbox>().OrderByDescending(x=>x.CreatedAt).Skip((page-1)*20).Take(20).Select(x=>new{x.Id,x.OrderId,x.Kind,x.State,x.Attempts,x.LastError,x.CreatedAt,x.ProcessedAt}).ToListAsync(ct);
 [HttpPost("{id}/retry")]public async Task<IActionResult> Retry(string id,CancellationToken ct)
 {var n=await db.Set<CommerceOutbox>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();if(n.State=="Failed"){n.State="Pending";n.AvailableAt=DateTime.UtcNow;n.Attempts=0;await db.SaveChangesAsync(ct);}return NoContent();}
}
