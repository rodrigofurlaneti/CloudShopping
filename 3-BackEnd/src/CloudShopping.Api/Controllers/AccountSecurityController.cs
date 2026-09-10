using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/session/security"), Authorize(Roles = "Administrator,Customer")]
public sealed class AccountSecurityController(AccountSecurity security) : ControllerBase
{
    private int Subject => StoreSecurity.Subject(User);
    private string Kind => User.IsInRole("Administrator") ? "Administrator" : "Customer";
    private string Current => User.FindFirst("sid")!.Value;

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(CancellationToken ct) => Ok(await security.Sessions(Subject, Kind, Current, ct));

    [HttpPost("sessions/revoke")]
    public async Task<IActionResult> Revoke(RevokeSessionInput input, CancellationToken ct)
    {
        await security.Revoke(Subject, Kind, Current, input.SessionId, ct);
        if (input.SessionId == Current) await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }

    [HttpPost("password")]
    public async Task<IActionResult> Password(ChangePasswordInput input, CancellationToken ct)
    {
        await security.ChangePassword(Subject, Kind, Current, input.CurrentPassword, input.NewPassword, ct);
        await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }
}
public sealed record RevokeSessionInput([RegularExpression("^[a-f0-9]{32}$")] string? SessionId);
public sealed record ChangePasswordInput([Required, MaxLength(72)] string CurrentPassword, [Required, MinLength(12), MaxLength(72)] string NewPassword);
