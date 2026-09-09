using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Domain.Entities.Backoffice;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CloudShopping.Api.Security;
public static class StoreSecurity
{
    public const string Scheme = "StoreSession";
    public static string Stamp(string? credential) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential ?? "")));
    public static int Subject(ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public static async Task Validate(CookieValidatePrincipalContext ctx)
    {
        var user = ctx.Principal!;
        if (!int.TryParse(user.FindFirstValue("tenant"), out var tenant)) { ctx.RejectPrincipal(); return; }
        ctx.HttpContext.Items["TenantId"] = tenant;
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var sid = user.FindFirstValue("sid");
        var session = await db.Set<AuthSession>().SingleOrDefaultAsync(x => x.Id == sid);
        if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow ||
            session.TenantId != tenant || !await db.Tenants.AnyAsync()) { ctx.RejectPrincipal(); return; }
        string? credential;
        bool active;
        if (session.Kind == "Administrator")
        {
            var employee = await db.Set<EmployeeUser>().SingleOrDefaultAsync(x => x.Id == session.SubjectId);
            active = employee?.IsActive == true && await db.Set<Employee>().AnyAsync(x => x.Id == employee.EmployeeId && x.IsActive);
            credential = employee?.PasswordHash;
            active &= await (from pu in db.Set<ProfileUser>() join p in db.Set<Profile>() on pu.ProfileId equals p.Id
                where pu.EmployeeUserId == session.SubjectId && pu.IsActive && p.IsActive && p.Name == "Administrador Geral"
                select pu.Id).AnyAsync();
        }
        else
        {
            var customer = await db.Customers.SingleOrDefaultAsync(x => x.Id == session.SubjectId);
            active = customer != null;
            credential = customer?.PasswordHash ?? customer?.SessionToken.ToString();
        }
        if (!active || session.CredentialStamp != Stamp(credential)) ctx.RejectPrincipal();
    }
    public static async Task SignIn(HttpContext http, AppDbContext db, int id, string role, string name, string credential, bool isGuest = false)
    {
        var session = new AuthSession { TenantId = db.CurrentTenantId, SubjectId = id, Kind = role,
            CredentialStamp = Stamp(credential), ExpiresAt = DateTime.UtcNow.AddHours(8) };
        db.Add(session); await db.SaveChangesAsync();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role), new Claim("tenant", db.CurrentTenantId.ToString()), new Claim("sid", session.Id), new Claim("guest", isGuest ? "true" : "false")
        }, Scheme));
        await http.SignInAsync(Scheme, principal, new AuthenticationProperties { ExpiresUtc = session.ExpiresAt, IsPersistent = false });
    }
    public static async Task ResolveTenant(HttpContext http, RequestDelegate next)
    {
        if (!http.Request.Path.StartsWithSegments("/api")) { await next(http); return; }
        var config = http.RequestServices.GetRequiredService<IConfiguration>();
        int tenant = 0;
        if (http.User.Identity?.IsAuthenticated == true) int.TryParse(http.User.FindFirstValue("tenant"), out tenant);
        var env = http.RequestServices.GetRequiredService<IHostEnvironment>();
        int requested = 0;
        if (env.IsDevelopment()) int.TryParse(http.Request.Headers["X-Tenant-Id"], out requested);
        else int.TryParse(config[$"Storefront:Hosts:{http.Request.Host.Host.ToLowerInvariant()}"], out requested);
        if (tenant > 0 && requested > 0 && tenant != requested) { http.Response.StatusCode = 403; return; }
        tenant = tenant > 0 ? tenant : requested;
        if (tenant <= 0) { http.Response.StatusCode = 400; await http.Response.WriteAsJsonAsync(new { message = "Loja não configurada." }); return; }
        http.Items["TenantId"] = tenant;
        // Cookie authentication may already have created a scoped context. Header mismatches were rejected above.
        if (http.Request.RouteValues.TryGetValue("tenantId", out var route) && route?.ToString() != tenant.ToString())
        { http.Response.StatusCode = 403; return; }
        await using var conn = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync(http.RequestAborted);
        await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM tenants WHERE Id=@id AND IsActive=1", conn);
        cmd.Parameters.AddWithValue("@id", tenant);
        if (Convert.ToInt32(await cmd.ExecuteScalarAsync(http.RequestAborted)) != 1) { http.Response.StatusCode = 404; return; }
        await next(http);
    }
}

