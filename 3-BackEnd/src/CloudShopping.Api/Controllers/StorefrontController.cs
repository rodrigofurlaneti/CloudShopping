using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Controllers;

[ApiController, Route("api/v1/store")]
public sealed class StorefrontController(AppDbContext db, StoreCommerceService commerce, CloudShopping.Infrastructure.Payments.AsaasPayments payments) : ControllerBase
{
    private int CustomerId => StoreSecurity.Subject(User);
    [HttpGet("context"), AllowAnonymous]
    public async Task<IActionResult> Context(CancellationToken ct) => Ok(await db.Tenants.Select(x => new { x.Id, x.CompanyName }).SingleAsync(ct));

    [HttpGet("departments"), AllowAnonymous]
    public async Task<IActionResult> Departments(CancellationToken ct) => Ok(await db.Departments.OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.Name, x.Slug }).ToListAsync(ct));

    [HttpGet("products"), AllowAnonymous]
    public async Task<IActionResult> Products(string? search = null, int? departmentId = null, int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var q = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.Contains(search) || x.Sku.Contains(search));
        if (departmentId != null) q = q.Where(x => x.DepartmentId == departmentId);
        var count = await q.CountAsync(ct);
        var items = await q.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.Name, sku = x.Sku, x.Price, x.DepartmentId, availableStock = x.PhysicalStock - x.ReservedStock,
                image = x.Images.Where(i => i.IsActive).OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.FilePath).FirstOrDefault() })
            .ToListAsync(ct);
        return Ok(new { items, totalCount = count, page, pageSize, totalPages = (int)Math.Ceiling((double)count / pageSize) });
    }
    [HttpGet("products/{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Product(int id, CancellationToken ct)
    {
        var p = await db.Products.AsNoTracking().Where(x => x.Id == id).Select(x => new {
            x.Id, x.Name, sku = x.Sku, x.Price, x.DepartmentId, availableStock = x.PhysicalStock - x.ReservedStock,
            images = x.Images.Where(i => i.IsActive).OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.FilePath).ToArray()
        }).SingleOrDefaultAsync(ct);
        return p == null ? NotFound(new { message = "Produto não encontrado." }) : Ok(p);
    }
    [HttpGet("cart"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cart(CancellationToken ct) => Ok(await commerce.ViewCart(CustomerId, ct));
    [HttpPost("cart/items"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Add(ItemInput input, CancellationToken ct) => Ok(await commerce.ChangeCart(CustomerId, input.ProductId, input.Quantity, "add", ct));
    [HttpPut("cart/items/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Set(int id, QuantityInput input, CancellationToken ct) => Ok(await commerce.ChangeCart(CustomerId, id, input.Quantity, "set", ct));
    [HttpDelete("cart/items/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct) => Ok(await commerce.ChangeCart(CustomerId, id, 0, "remove", ct));
    [HttpDelete("cart"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Clear(CancellationToken ct) => Ok(await commerce.ChangeCart(CustomerId, 0, 0, "clear", ct));

    [HttpGet("profile"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var c = await db.Customers.Include(x => x.Individual).Include(x => x.Company).SingleAsync(x => x.Id == CustomerId, ct);
        return Ok(new { c.Email, type = c.Company == null ? "B2C" : "B2B",
            name = c.Individual?.FullName ?? c.Company?.CompanyName ?? "", taxId = c.Individual?.TaxId ?? c.Company?.BusinessTaxId ?? "" });
    }
    [HttpPut("profile"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateProfile(ProfileInput input, CancellationToken ct)
    {
        if (!TaxDocument.IsValid(input.TaxId) || (input.Type == "B2C" ? input.TaxId.Length != 11 : input.TaxId.Length != 14))
            return BadRequest(new { message = "CPF/CNPJ inválido." });
        var email = input.Email.Trim().ToLowerInvariant();
        if (await db.Customers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == db.CurrentTenantId && x.Email == email && x.Id != CustomerId, ct))
            return Conflict(new { message = "Email já cadastrado. Entre com sua conta antes de continuar." });
        var c = await db.Customers.Include(x => x.Individual).Include(x => x.Company).SingleAsync(x => x.Id == CustomerId, ct);
        c.ChangeEmail(email);
        if (input.Type == "B2C")
        {
            if (c.Individual == null) c.RegisterAsB2C(input.TaxId, input.Name.Trim(), null);
            else { if (c.Individual.TaxId != input.TaxId) return Conflict(new { message = "O documento cadastrado não pode ser trocado nesta tela." }); c.UpdateB2CProfile(input.Name.Trim(), c.Individual.BirthDate); }
        }
        else
        {
            if (c.Company == null) c.RegisterAsB2B(input.TaxId, input.Name.Trim(), null);
            else { if (c.Company.BusinessTaxId != input.TaxId) return Conflict(new { message = "O documento cadastrado não pode ser trocado nesta tela." }); c.UpdateB2BProfile(input.Name.Trim(), c.Company.StateTaxId); }
        }
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("addresses"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Addresses(CancellationToken ct) => Ok(await db.Addresses.Where(x => x.CustomerId == CustomerId && x.IsActive)
        .Select(x => new { x.Id, x.Street, x.Number, x.Neighborhood, x.City, x.State, x.ZipCode, x.IsDefault }).ToListAsync(ct));
    [HttpPost("addresses"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddAddress(AddressInput input, CancellationToken ct)
    {
        var a = Address.Create(CustomerId, AddressType.Shipping, input.Street, input.Number, input.Neighborhood,
            input.City, input.State, input.ZipCode, false);
        db.Add(a); await db.SaveChangesAsync(ct); return Ok(new { a.Id });
    }

    [HttpGet("shipping-options"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Shipping(int addressId, CancellationToken ct)
    {
        var address = await db.Addresses.SingleOrDefaultAsync(x => x.Id == addressId && x.CustomerId == CustomerId && x.IsActive, ct);
        if (address == null) return NotFound();
        return Ok(await db.Set<ShippingOption>().Where(x => x.IsActive && address.ZipCode.StartsWith(x.PostalCodePrefix))
            .Select(x => new { x.Id, x.Name, x.Amount, x.EstimatedDays }).ToListAsync(ct));
    }
    [HttpPost("checkout/preview"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Preview(PreviewInput input, CancellationToken ct) => Ok(await commerce.Preview(CustomerId, input.AddressId, input.ShippingId, ct));
    [HttpPost("checkout/confirm"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Confirm(ConfirmInput input, CancellationToken ct) => Ok(await commerce.Confirm(CustomerId, input.Key, input.Token, ct));
    [HttpGet("orders"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Orders(int page = 1, CancellationToken ct = default) => Ok(await db.Orders.Where(x => x.CustomerId == CustomerId)
        .OrderByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).Select(x => new { x.Id, x.TotalAmount, x.OrderStatusId, x.ReservationState, x.ReservationExpiresAt }).ToListAsync(ct));
    [HttpGet("orders/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Order(int id, CancellationToken ct) => Ok(await commerce.GetOrder(CustomerId, id, ct));
    [HttpPost("orders/{id:int}/cancel"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) { await payments.Reconcile(id, CustomerId, ct,"cancel","Customer:"+CustomerId); return NoContent(); }

    [HttpGet("admin/shipping-options"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdminShipping(CancellationToken ct) => Ok(await db.Set<ShippingOption>().ToListAsync(ct));
    [HttpPost("admin/shipping-options"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SaveShipping(ShippingInput input, CancellationToken ct)
    {
        var option = new ShippingOption { TenantId = db.CurrentTenantId, Name = input.Name.Trim(), Amount = input.Amount,
            EstimatedDays = input.EstimatedDays, PostalCodePrefix = input.PostalCodePrefix };
        db.Add(option); await db.SaveChangesAsync(ct); return Ok(new { option.Id });
    }
    [HttpDelete("admin/shipping-options/{id:int}"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DisableShipping(int id, CancellationToken ct)
    {
        var option = await db.Set<ShippingOption>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (option == null) return NotFound(); option.IsActive = false; await db.SaveChangesAsync(ct); return NoContent();
    }
}
public sealed record ItemInput([Range(1,int.MaxValue)] int ProductId, [Range(1,999)] int Quantity);
public sealed record QuantityInput([Range(1,999)] int Quantity);
public sealed record ProfileInput([Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MaxLength(150)] string Name, [Required, RegularExpression("^(B2C|B2B)$")] string Type,
    [Required, RegularExpression("^[0-9]{11}([0-9]{3})?$")] string TaxId);
public sealed record AddressInput([Required, MaxLength(150)] string Street, [Required, MaxLength(10)] string Number,
    [MaxLength(50)] string? Neighborhood, [Required, MaxLength(50)] string City,
    [Required, RegularExpression("^[A-Za-z]{2}$")] string State, [Required, RegularExpression("^[0-9]{8}$")] string ZipCode);
public sealed record PreviewInput([Range(1,int.MaxValue)] int AddressId, [Range(1,int.MaxValue)] int ShippingId);
public sealed record ConfirmInput([Required, MaxLength(64)] string Key, [Required, MaxLength(16000)] string Token);
public sealed record ShippingInput([Required, MaxLength(100)] string Name, [Range(typeof(decimal),"0","999999.99")] decimal Amount,
    [RegularExpression("^[0-9]{0,8}$")] string PostalCodePrefix, [Range(0,365)] int EstimatedDays);
internal static class TaxDocument
{
    public static bool IsValid(string value)
    {
        if (value.Length is not (11 or 14) || !value.All(char.IsAsciiDigit) || value.Distinct().Count() == 1) return false;
        int Calculate(int length) {
            var sum = 0;
            for (int i = 0; i < length; i++) {
                int weight = value.Length == 11 ? length + 1 - i : (length - 1 - i) % 8 + 2;
                sum += (value[i] - '0') * weight;
            }
            int rest = sum % 11; return rest < 2 ? 0 : 11 - rest;
        }
        return Calculate(value.Length - 2) == value[^2] - '0' && Calculate(value.Length - 1) == value[^1] - '0';
    }
}
