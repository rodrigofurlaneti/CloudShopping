using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using MediatR;
using CloudShopping.Application.Features.AccountSecurity.Commands.ChangeAccountPassword;
using CloudShopping.Application.Features.AccountSecurity.Commands.RevokeAccountSessions;
using CloudShopping.Application.Features.AccountSecurity.Queries.GetAccountSessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/session/security"), Authorize(Roles = "Administrator,Customer")]
public sealed class AccountSecurityController(ISender sender) : ControllerBase
{
    private int Subject => StoreSecurity.Subject(User);
    private string Kind => User.IsInRole("Administrator") ? "Administrator" : "Customer";
    private string Current => User.FindFirst("sid")!.Value;

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetAccountSessionsQuery(Subject,Kind,Current),ct), value => Ok(value));

    [HttpPost("sessions/revoke")]
    public async Task<IActionResult> Revoke(RevokeSessionInput input, CancellationToken ct)
    {
        var result=await sender.Send(new RevokeAccountSessionsCommand(Subject,Kind,Current,input.SessionId),ct);
        if(!result.IsSuccess)return CommandResults.Respond(result,_=>NoContent());
        if (input.SessionId == Current) await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }

    [HttpPost("password")]
    public async Task<IActionResult> Password(ChangePasswordInput input, CancellationToken ct)
    {
        var result=await sender.Send(new ChangeAccountPasswordCommand(Subject,Kind,Current,input.CurrentPassword,input.NewPassword),ct);
        if(!result.IsSuccess)return CommandResults.Respond(result,_=>NoContent());
        await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }
}
public sealed record RevokeSessionInput([RegularExpression("^[a-f0-9]{32}$")] string? SessionId);
public sealed record ChangePasswordInput([Required, MaxLength(72)] string CurrentPassword, [Required, MinLength(12), MaxLength(72)] string NewPassword);
