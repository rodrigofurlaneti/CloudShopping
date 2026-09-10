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
public sealed class SessionController(SessionUseCases sessions, ITenantProvider tenant, IAntiforgery csrf) : ControllerBase
{
    private SessionCaller Caller => new(User.Identity?.IsAuthenticated == true ? StoreSecurity.Subject(User) : 0,
        User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value : null, User.FindFirst("sid")?.Value);

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Current(CancellationToken ct) => Ok(new {
        csrfToken = csrf.GetAndStoreTokens(HttpContext).RequestToken,
        user = User.Identity?.IsAuthenticated == true ? new {
            id = Caller.Id, name = User.Identity.Name, role = Caller.Role, tenantId = tenant.GetTenantId(),
            isGuest = User.FindFirst("guest")?.Value == "true", permissions = await sessions.Permissions(Caller, ct)
        } : null
    });

    [HttpPost("admin/login"), AllowAnonymous]
    public async Task<IActionResult> AdminLogin(LoginInput input, CancellationToken ct)
    {
        var result = await sessions.AdminLogin(Caller, input.Username, input.Password, ct);
        if (result == null) return Unauthorized(new { message = "Credenciais inválidas." });
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id, name = result.Name, role = result.Role, tenantId = tenant.GetTenantId() });
    }

    [HttpPost("guest"), AllowAnonymous]
    public async Task<IActionResult> Guest(CancellationToken ct)
    {
        var result = await sessions.Guest(Caller, ct);
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken ct)
    {
        var result = await sessions.Register(Caller, input.Email, input.Password, ct);
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> CustomerLogin(LoginInput input, CancellationToken ct)
    {
        var result = await sessions.CustomerLogin(Caller, input.Username, input.Password, ct);
        if (result == null) return Unauthorized(new { message = "Credenciais inválidas." });
        await StoreSecurity.SignIn(HttpContext, result);
        return Ok(new { id = result.Id });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await sessions.Logout(Caller, ct);
        await HttpContext.SignOutAsync(StoreSecurity.Scheme);
        return NoContent();
    }
}
public sealed record LoginInput([Required, MaxLength(100)] string Username, [Required, MaxLength(72)] string Password);
public sealed record RegisterInput([Required, EmailAddress, MaxLength(100)] string Email, [Required, MinLength(12), MaxLength(72)] string Password);
