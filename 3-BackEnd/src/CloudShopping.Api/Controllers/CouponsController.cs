using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;
[ApiController,Route("api/v1/coupons"),Authorize(Roles="Administrator")]
public sealed class CouponsController(AppDbContext db):ControllerBase
{
 [HttpGet]public async Task<object> List(int page=1,CancellationToken ct=default)=>await db.Set<Coupon>().OrderByDescending(x=>x.StartsAt).Skip((page-1)*20).Take(20).ToListAsync(ct);
 [HttpPost]public Task<Coupon> Create(CouponInput input,CancellationToken ct)=>new Coupons(db).Create(input,ct);
 [HttpPut("{id}/enabled")]public async Task<IActionResult> Enable(string id,EnableCouponInput input,CancellationToken ct)
 {var c=await db.Set<Coupon>().SingleOrDefaultAsync(x=>x.Id==id,ct)??throw new KeyNotFoundException();if(c.Enabled==input.Enabled)return NoContent();if(c.Version!=input.Version)throw new CommerceConflictException("Cupom alterado; recarregue.");c.Enabled=input.Enabled;await db.SaveChangesAsync(ct);return NoContent();}
}
public sealed record EnableCouponInput(int Version,bool Enabled);
