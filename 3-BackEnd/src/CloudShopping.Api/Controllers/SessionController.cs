using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/session")]
public sealed class SessionController(AppDbContext db, IPasswordHasher hasher, IAntiforgery csrf) : ControllerBase
{
    [HttpGet, AllowAnonymous]
    public IActionResult Current() => Ok(new {
        csrfToken = csrf.GetAndStoreTokens(HttpContext).RequestToken,
        user = User.Identity?.IsAuthenticated == true ? new {
            id = StoreSecurity.Subject(User), name = User.Identity.Name,
            role = User.IsInRole("Administrator") ? "Administrator" : "Customer", tenantId = db.CurrentTenantId, isGuest = User.FindFirst("guest")?.Value == "true"
        } : null
    });

    [HttpPost("admin/login"), AllowAnonymous]
    public async Task<IActionResult> AdminLogin(LoginInput input, CancellationToken ct)
    {
        var username = input.Username.Trim();
        var employee = await db.Set<EmployeeUser>().SingleOrDefaultAsync(x => x.Username == username && x.IsActive, ct);
        if (employee == null || !hasher.Verify(input.Password, employee.PasswordHash)) return Unauthorized(new { message = "Credenciais inválidas." });
        if (!await db.Set<Employee>().AnyAsync(x => x.Id == employee.EmployeeId && x.IsActive, ct)) return Forbid();
        var permitted = await (from pu in db.Set<ProfileUser>() join p in db.Set<Profile>() on pu.ProfileId equals p.Id
            where pu.EmployeeUserId == employee.Id && pu.IsActive && p.IsActive && p.Name == "Administrador Geral" select pu.Id).AnyAsync(ct);
        if (!permitted) return Forbid();
        await RevokeCurrent(ct);
        await StoreSecurity.SignIn(HttpContext, db, employee.Id, "Administrator", employee.Username, employee.PasswordHash);
        return Ok(new { id = employee.Id, name = employee.Username, role = "Administrator", tenantId = db.CurrentTenantId });
    }

    [HttpPost("guest"), AllowAnonymous]
    public async Task<IActionResult> Guest(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
            return User.IsInRole("Customer") ? Ok(new { id = StoreSecurity.Subject(User) }) : Conflict(new { message = "Saia da sessão administrativa para comprar." });
        var guest = Customer.CreateGuest(db.CurrentTenantId);
        db.Add(guest); await db.SaveChangesAsync(ct);
        await StoreSecurity.SignIn(HttpContext, db, guest.Id, "Customer", "Visitante", guest.SessionToken.ToString(), isGuest: true);
        return Ok(new { id = guest.Id });
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterInput input, CancellationToken ct)
    {
        if (User.IsInRole("Administrator")) return Forbid();
        if (System.Text.Encoding.UTF8.GetByteCount(input.Password) > 72) return BadRequest(new { message = "Senha excede 72 bytes." });
        var email = input.Email.Trim().ToLowerInvariant();
        var currentId = User.IsInRole("Customer") ? StoreSecurity.Subject(User) : 0;
        if (await db.Customers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == db.CurrentTenantId && x.Email == email && x.Id != currentId, ct))
            return Conflict(new { message = "Cadastro indisponível para este email. Entre com sua conta." });
        var customer = User.IsInRole("Customer") ? await db.Customers.SingleAsync(x => x.Id == StoreSecurity.Subject(User), ct) : Customer.CreateGuest(db.CurrentTenantId);
        if (customer.PasswordHash != null) return Conflict(new { message = "A conta já está cadastrada." });
        customer.ChangeEmail(email); customer.SetPassword(hasher.Hash(input.Password));
        if (customer.Id == 0) db.Add(customer);
        await db.SaveChangesAsync(ct); await RevokeCurrent(ct);
        await StoreSecurity.SignIn(HttpContext, db, customer.Id, "Customer", email, customer.PasswordHash!);
        return Ok(new { id = customer.Id });
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> CustomerLogin(LoginInput input, CancellationToken ct)
    {
        if (User.IsInRole("Administrator")) return Forbid();
        var email = input.Username.Trim().ToLowerInvariant();
        var customer = await db.Customers.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (customer?.PasswordHash == null || !hasher.Verify(input.Password, customer.PasswordHash)) return Unauthorized(new { message = "Credenciais inválidas." });
        // Keep the guest cart isolated; explicitly merge only after proving the destination identity.
        if (User.IsInRole("Customer") && StoreSecurity.Subject(User) != customer.Id)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var guestId = StoreSecurity.Subject(User);
            var guest = await db.Customers.SingleAsync(x => x.Id == guestId, ct);
            if (guest.PasswordHash == null)
            {
                var source = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.CustomerId == guestId, ct);
                var dest = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.CustomerId == customer.Id, ct);
                if (source != null && source.ExpiresAt > DateTime.UtcNow && source.Items.Count > 0)
                {
                    if (dest == null) { dest = CloudShopping.Domain.Entities.Carts.Cart.Create(customer.Id); db.Add(dest); }
                    else if (dest.ExpiresAt <= DateTime.UtcNow) dest.Clear();
                    foreach (var item in source.Items)
                    {
                        var p = await db.Products.SingleOrDefaultAsync(x => x.Id == item.ProductId, ct);
                        if (p != null) {
                            var existingQuantity = dest.Items.SingleOrDefault(x => x.ProductId == p.Id)?.Quantity ?? 0;
                            var addition = Math.Min(item.Quantity, 999 - existingQuantity);
                            if (addition > 0) dest.AddOrUpdateItem(p.Id, addition, p.Price);
                        }
                    }
                    source.Clear(); await db.SaveChangesAsync(ct);
                }
            }
            await tx.CommitAsync(ct);
        }
        await RevokeCurrent(ct);
        await StoreSecurity.SignIn(HttpContext, db, customer.Id, "Customer", email, customer.PasswordHash);
        return Ok(new { id = customer.Id });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await RevokeCurrent(ct); await HttpContext.SignOutAsync(StoreSecurity.Scheme); return NoContent();
    }
    private async Task RevokeCurrent(CancellationToken ct)
    {
        var sid = User.FindFirst("sid")?.Value;
        var session = sid == null ? null : await db.Set<AuthSession>().SingleOrDefaultAsync(x => x.Id == sid && x.TenantId == db.CurrentTenantId, ct);
        if (session != null) { session.RevokedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
    }
}
public sealed record LoginInput([Required, MaxLength(100)] string Username, [Required, MaxLength(72)] string Password);
public sealed record RegisterInput([Required, EmailAddress, MaxLength(100)] string Email, [Required, MinLength(12), MaxLength(72)] string Password);
