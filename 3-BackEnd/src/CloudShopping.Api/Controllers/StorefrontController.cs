using System.ComponentModel.DataAnnotations;
using CloudShopping.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreContext;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreDepartments;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreProducts;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreProduct;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreCart;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreProfile;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreAddresses;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreShipping;
using CloudShopping.Application.Features.Storefront.Queries.PreviewStoreCheckout;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreOrders;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreOrder;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreAdminShipping;
using CloudShopping.Application.Features.Storefront.Commands.ChangeStoreCart;
using CloudShopping.Application.Features.Storefront.Commands.UpdateStoreProfile;
using CloudShopping.Application.Features.Storefront.Commands.AddStoreAddress;
using CloudShopping.Application.Features.Storefront.Commands.ConfirmStoreCheckout;
using CloudShopping.Application.Features.Storefront.Commands.CancelStoreOrder;
using CloudShopping.Application.Features.Storefront.Commands.AddStoreShipping;
using CloudShopping.Application.Features.Storefront.Commands.DisableStoreShipping;
namespace CloudShopping.Api.Controllers;
[ApiController, Route("api/v1/store")]
public sealed class StorefrontController(ISender sender) : ControllerBase
{
    [HttpGet("postal-address/{zipCode}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> PostalAddress(string zipCode, CancellationToken ct)
    {
        var address = await sender.Send(new CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress.GetPostalAddressQuery(zipCode), ct);
        return CommandResults.Respond(address, value => value == null ? NotFound(new { message = "CEP não encontrado. Preencha o endereço manualmente." }) : Ok(value));
    }
    private int CustomerId => StoreSecurity.Subject(User);
    [HttpGet("context"), AllowAnonymous]
    public async Task<IActionResult> Context(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreContextQuery(), ct), value => Ok(value));
    [HttpGet("departments"), AllowAnonymous]
    public async Task<IActionResult> Departments(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreDepartmentsQuery(), ct), value => Ok(value));
    [HttpGet("products"), AllowAnonymous]
    public async Task<IActionResult> Products(string? search = null, int? departmentId = null, int page = 1, int pageSize = 12, CancellationToken ct = default)
        => CommandResults.Respond(await sender.Send(new GetStoreProductsQuery(search, departmentId, page, Math.Clamp(pageSize,1,100)), ct), value => Ok(value));
    [HttpGet("products/{id:int}"), AllowAnonymous]
    public async Task<IActionResult> Product(int id, CancellationToken ct) {
        var product = await sender.Send(new GetStoreProductQuery(id, null), ct);
        return CommandResults.Respond(product, value => value == null ? NotFound(new { message = "Produto não encontrado." }) : Ok(value));
    }
    [HttpGet("products/by-slug/{slug}"), AllowAnonymous]
    public async Task<IActionResult> ProductBySlug(string slug, CancellationToken ct) {
        var product = await sender.Send(new GetStoreProductQuery(null, slug), ct);
        return CommandResults.Respond(product, value => value == null ? NotFound(new { message = "Produto não encontrado." }) : Ok(value));
    }
    [HttpGet("cart"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cart(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreCartQuery(CustomerId), ct), value => Ok(value));
    [HttpPost("cart/items"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Add(ItemInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new ChangeStoreCartCommand(CustomerId,input.ProductId,input.Quantity,"add"), ct), value => Ok(value));
    [HttpPut("cart/items/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Set(int id, QuantityInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new ChangeStoreCartCommand(CustomerId,id,input.Quantity,"set"), ct), value => Ok(value));
    [HttpDelete("cart/items/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct) => CommandResults.Respond(await sender.Send(new ChangeStoreCartCommand(CustomerId,id,0,"remove"), ct), value => Ok(value));
    [HttpDelete("cart"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Clear(CancellationToken ct) => CommandResults.Respond(await sender.Send(new ChangeStoreCartCommand(CustomerId,0,0,"clear"), ct), value => Ok(value));
    [HttpGet("profile"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Profile(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreProfileQuery(CustomerId), ct), value => Ok(value));
    [HttpPut("profile"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateProfile(ProfileInput input, CancellationToken ct) {
        return CommandResults.Respond(await sender.Send(new UpdateStoreProfileCommand(CustomerId,input.Email,input.Name,input.Type,input.TaxId), ct), _ => NoContent());
    }
    [HttpGet("addresses"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Addresses(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreAddressesQuery(CustomerId), ct), value => Ok(value));
    [HttpPost("addresses"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddAddress(AddressInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new AddStoreAddressCommand(CustomerId,input.Street,input.Number,input.Neighborhood,input.City,input.State,input.ZipCode), ct), id => Ok(new { id }));
    [HttpGet("shipping-options"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Shipping(int addressId, CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreShippingQuery(CustomerId,addressId), ct), value => Ok(value));
    [HttpPost("checkout/preview"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Preview(PreviewInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new PreviewStoreCheckoutQuery(CustomerId,input.AddressId,input.ShippingId,input.CouponCode), ct), value => Ok(value));
    [HttpPost("checkout/confirm"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Confirm(ConfirmInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new ConfirmStoreCheckoutCommand(CustomerId,input.Key,input.Token), ct), value => Ok(value));
    [HttpGet("orders"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Orders(int page = 1, CancellationToken ct = default) => CommandResults.Respond(await sender.Send(new GetStoreOrdersQuery(CustomerId,page), ct), value => Ok(value));
    [HttpGet("orders/{id:int}"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Order(int id, CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreOrderQuery(CustomerId,id), ct), value => Ok(value));
    [HttpPost("orders/{id:int}/cancel"), Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) { return CommandResults.Respond(await sender.Send(new CancelStoreOrderCommand(CustomerId,id), ct), _ => NoContent()); }
    [HttpGet("admin/shipping-options"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AdminShipping(CancellationToken ct) => CommandResults.Respond(await sender.Send(new GetStoreAdminShippingQuery(), ct), value => Ok(value));
    [HttpPost("admin/shipping-options"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SaveShipping(ShippingInput input, CancellationToken ct) => CommandResults.Respond(await sender.Send(new AddStoreShippingCommand(input.Name,input.Amount,input.EstimatedDays,input.PostalCodePrefix), ct), id => Ok(new { id }));
    [HttpDelete("admin/shipping-options/{id:int}"), Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DisableShipping(int id, CancellationToken ct) { return CommandResults.Respond(await sender.Send(new DisableStoreShippingCommand(id), ct), _ => NoContent()); }
}
public sealed record ItemInput([Range(1,int.MaxValue)] int ProductId, [Range(1,999)] int Quantity);
public sealed record QuantityInput([Range(1,999)] int Quantity);
public sealed record ProfileInput([Required, EmailAddress, MaxLength(100)] string Email,
    [Required, MaxLength(150)] string Name, [Required, RegularExpression("^(B2C|B2B)$")] string Type,
    [Required, RegularExpression("^[0-9]{11}([0-9]{3})?$")] string TaxId);
public sealed record AddressInput([Required, MaxLength(150)] string Street, [Required, MaxLength(10)] string Number,
    [MaxLength(50)] string? Neighborhood, [Required, MaxLength(50)] string City,
    [Required, RegularExpression("^[A-Za-z]{2}$")] string State, [Required, RegularExpression("^[0-9]{8}$")] string ZipCode);
public sealed record PreviewInput([Range(1,int.MaxValue)] int AddressId, [Range(1,int.MaxValue)] int ShippingId,[MaxLength(40)]string? CouponCode=null);
public sealed record ConfirmInput([Required, MaxLength(64)] string Key, [Required, MaxLength(16000)] string Token);
public sealed record ShippingInput([Required, MaxLength(100)] string Name, [Range(typeof(decimal),"0","999999.99")] decimal Amount,
    [RegularExpression("^[0-9]{0,8}$")] string PostalCodePrefix, [Range(0,365)] int EstimatedDays);
