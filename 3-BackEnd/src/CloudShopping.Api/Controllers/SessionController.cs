using MediatR;
using CloudShopping.Application.Features.Sessions.Queries.GetSessionPermissions;
using CloudShopping.Application.Features.Sessions.Commands.AdminSessionLogin;
using CloudShopping.Application.Features.Sessions.Commands.CustomerSessionLogin;
using CloudShopping.Application.Features.Sessions.Commands.CreateGuestSession;
using CloudShopping.Application.Features.Sessions.Commands.RegisterSessionAccount;
using CloudShopping.Application.Features.Sessions.Commands.LogoutSession;
using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Sessions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/session")]
public sealed class SessionController(ISender sender, ITenantProvider tenant, IAntiforgery csrf) : ControllerBase
{
    private SessionCaller Caller => new(User.Identity?.IsAuthenticated == true ? StoreSecurity.Subject(User) : 0,
        User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value : null, User.FindFirst("sid")?.Value);

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Current(CancellationToken ct)
    {
        var permissions = await sender.Send(new GetSessionPermissionsQuery(Caller), ct);
        if (permissions.IsFailure) return CommandResults.Respond(permissions, _ => NoContent());
        return Ok(new {
        csrfToken = csrf.GetAndStoreTokens(HttpContext).RequestToken,
        user = User.Identity?.IsAuthenticated == true ? new {
            id = Caller.Id, name = User.Identity.Name, role = Caller.Role, tenantId = tenant.GetTenantId(),
            isGuest = User.FindFirst("guest")?.Value == "true", permissions = permissions.Value
        } : null
    });
    }

    [HttpPost("admin/login"), AllowAnonymous]
    public async Task<IActionResult> AdminLogin(LoginInput input, CancellationToken ct)
    {
        var outcome = await sender.Send(new AdminSessionLoginCommand(Caller,input.Username,input.Password),ct);
        if(!outcome.IsSuccess)return CommandResults.Respond(outcome,_=>NoContent());
        var result=outcome.Value;
        if (result == null) return Unauthorized(new { message = "Credenciais inválidas." });
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id, name = result.Name, role = result.Role, tenantId = tenant.GetTenantId() });
    }

    [HttpPost("guest"), AllowAnonymous]
    public async Task<IActionResult> Guest(CancellationToken ct)
    {
        var outcome = await sender.Send(new CreateGuestSessionCommand(Caller),ct);
        if(!outcome.IsSuccess)return CommandResults.Respond(outcome,_=>NoContent());
        var result=outcome.Value;
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken ct)
    {
        var outcome = await sender.Send(new RegisterSessionAccountCommand(Caller,input.Email,input.Password),ct);
        if(!outcome.IsSuccess)return CommandResults.Respond(outcome,_=>NoContent());
        var result=outcome.Value;
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> CustomerLogin(LoginInput input, CancellationToken ct)
    {
        var outcome = await sender.Send(new CustomerSessionLoginCommand(Caller,input.Username,input.Password),ct);
        if(!outcome.IsSuccess)return CommandResults.Respond(outcome,_=>NoContent());
        var result=outcome.Value;
        if (result == null) return Unauthorized(new { message = "Credenciais inválidas." });
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var outcome=await sender.Send(new LogoutSessionCommand(Caller),ct);
        if(!outcome.IsSuccess)return CommandResults.Respond(outcome,_=>NoContent());
        await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }
}
public sealed record LoginInput([Required, MaxLength(100)] string Username, [Required, MaxLength(72)] string Password);
public sealed record RegisterInput([Required, EmailAddress, MaxLength(100)] string Email, [Required, MinLength(12), MaxLength(72)] string Password);
