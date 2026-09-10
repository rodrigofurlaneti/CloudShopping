using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudShopping.Api.Controllers;
[ApiController, Route("api/v1/access")]
public sealed class AccessController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<object> List(CancellationToken ct)
    {
        var profiles = await db.Set<Profile>().OrderBy(x => x.Name).Select(x => new { x.Id, x.Name, x.IsActive }).ToListAsync(ct);
        var permissions = await db.Set<ProfilePermission>().ToListAsync(ct);
        return new { available = StorePermissions.Available, profiles = profiles.Select(x => new { x.Id, x.Name, x.IsActive, permissions = permissions.Where(p => p.ProfileId == x.Id).Select(p => p.Permission).Order().ToArray() }) };
    }
    [HttpPut("profiles/{id:int}")]
    public async Task<IActionResult> Replace(int id, PermissionsInput input, CancellationToken ct)
    {
        await new StorePermissions(db).Replace(StoreSecurity.Subject(User), id, input.Expected, input.Permissions, ct);
        return NoContent();
    }
    [HttpGet("changes")]
    public async Task<object> Changes(int page = 1, CancellationToken ct = default) => await db.Set<AccessChange>()
        .OrderByDescending(x => x.CreatedAt).Skip((page - 1) * 20).Take(20).ToListAsync(ct);
}
public sealed record PermissionsInput([Required, MaxLength(30)] string[] Expected, [Required, MaxLength(30)] string[] Permissions);
