using System.Security.Claims;
using CloudShopping.Application.Features.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CloudShopping.Api.Security;

// HTTP adapter only: cookies, claims, request metadata and HTTP responses.
public static class StoreSecurity
{
    public const string Scheme = "StoreSession";
    public static int Subject(ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static async Task Validate(CookieValidatePrincipalContext ctx)
    {
        var user = ctx.Principal!;
        if (!int.TryParse(user.FindFirstValue("tenant"), out var tenant) ||
            !int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var subject))
        { ctx.RejectPrincipal(); return; }
        ctx.HttpContext.Items["TenantId"] = tenant;
        var sessions = ctx.HttpContext.RequestServices.GetRequiredService<SessionLifecycle>();
        if (!await sessions.Validate(tenant, user.FindFirstValue("sid"), subject,
            user.FindFirstValue(ClaimTypes.Role) ?? "", ctx.HttpContext.RequestAborted)) ctx.RejectPrincipal();
    }

    public static async Task SignIn(HttpContext http, SessionLogin result)
    {
        if (result.Session is not { } session) return;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, result.Id.ToString()), new Claim(ClaimTypes.Name, result.Name),
            new Claim(ClaimTypes.Role, result.Role), new Claim("tenant", session.TenantId.ToString()), new Claim("sid", session.Id), new Claim("guest", result.IsGuest ? "true" : "false")
        }, Scheme));
        await http.SignInAsync(Scheme, principal, new AuthenticationProperties { ExpiresUtc = session.ExpiresAt, IsPersistent = false });
    }

    public static async Task ResolveTenant(HttpContext http, RequestDelegate next)
    {
        if (http.GetEndpoint()?.Metadata.GetMetadata<Controllers.AsaasWebhookAttribute>() != null || !http.Request.Path.StartsWithSegments("/api"))
        { await next(http); return; }
        var config = http.RequestServices.GetRequiredService<IConfiguration>();
        var env = http.RequestServices.GetRequiredService<IHostEnvironment>();
        int tenant = 0, requested = 0;
        if (http.User.Identity?.IsAuthenticated == true) int.TryParse(http.User.FindFirstValue("tenant"), out tenant);
        if (env.IsDevelopment()) int.TryParse(http.Request.Headers["X-Tenant-Id"], out requested);
        else int.TryParse(config[$"Storefront:Hosts:{http.Request.Host.Host.ToLowerInvariant()}"], out requested);
        int? routeTenant = http.Request.RouteValues.TryGetValue("tenantId", out var route)
            ? int.TryParse(route?.ToString(), out var routeId) ? routeId : -1 : null;
        var result = await http.RequestServices.GetRequiredService<SessionLifecycle>().ResolveTenant(tenant, requested, routeTenant, http.RequestAborted);
        if (result.Failure != TenantResolutionFailure.None)
        {
            http.Response.StatusCode = result.Failure switch { TenantResolutionFailure.Missing => 400, TenantResolutionFailure.Forbidden => 403, _ => 404 };
            if (result.Failure == TenantResolutionFailure.Missing) await http.Response.WriteAsJsonAsync(new { message = "Loja não configurada." });
            return;
        }
        http.Items["TenantId"] = result.TenantId;
        await next(http);
    }
}
