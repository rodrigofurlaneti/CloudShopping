using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace CloudShopping.Infrastructure.Services;

public sealed class CommerceConflictException(string message) : Exception(message);
public sealed record CartLine(int ProductId, string Name, string Sku, decimal Price, int Quantity, int AvailableStock, string? Image);
public sealed record CartView(int Id, int Version, DateTime ExpiresAt, IReadOnlyList<CartLine> Items, decimal Subtotal);
public sealed record Quote(int TenantId, int CustomerId, int CartId, int CartVersion, int AddressId, int ShippingId,
    string Fingerprint, decimal Total, DateTime ExpiresAt);
public sealed record CheckoutPreview(string Token, CartView Cart, string ShippingName, decimal ShippingAmount, decimal Total, DateTime ExpiresAt);
public sealed record PlacedOrder(int Id, decimal TotalAmount, decimal ShippingAmount, string? ShippingMethod,
    int OrderStatusId, string ReservationState, DateTime? ReservationExpiresAt, object[] Items, object? Address);

public sealed class StoreCommerceService(AppDbContext db, IDataProtectionProvider protection, IConfiguration config)
{
    private readonly IDataProtector protector = protection.CreateProtector("CloudShopping.Checkout.v1");
    private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public async Task<Cart> GetCart(int customerId, CancellationToken ct)
    {
        if (!await db.Customers.AnyAsync(x => x.Id == customerId, ct)) throw new UnauthorizedAccessException();
        var cart = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.CustomerId == customerId, ct);
        if (cart == null)
        {
            cart = Cart.Create(customerId); db.Add(cart);
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException) {
                db.ChangeTracker.Clear();
                cart = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.CustomerId == customerId, ct)
                    ?? throw new CommerceConflictException("Tente abrir o carrinho novamente.");
            }
        }
        if (cart.ExpiresAt <= DateTime.UtcNow) { cart.Clear(); await db.SaveChangesAsync(ct); }
        return cart;
    }
    public async Task<CartView> ViewCart(int customerId, CancellationToken ct)
    {
        var cart = await GetCart(customerId, ct);
        var ids = cart.Items.Select(x => x.ProductId).ToArray();
        var products = await db.Products.Include(x => x.Images).Where(x => ids.Contains(x.Id)).ToListAsync(ct);
        var lines = cart.Items.Select(item => {
            var p = products.SingleOrDefault(x => x.Id == item.ProductId);
            return new CartLine(item.ProductId, p?.Name ?? "Produto indisponível", p?.Sku ?? "",
                p?.Price ?? 0, item.Quantity, p?.AvailableStock ?? 0,
                p?.Images.Where(x => x.IsActive).OrderByDescending(x => x.IsPrimary).ThenBy(x => x.DisplayOrder).FirstOrDefault()?.FilePath);
        }).ToArray();
        return new(cart.Id, cart.Version, cart.ExpiresAt, lines, lines.Sum(x => x.Price * x.Quantity));
    }
    public async Task<CartView> ChangeCart(int customerId, int productId, int quantity, string operation, CancellationToken ct)
    {
        var cart = await GetCart(customerId, ct);
        if (operation == "clear") cart.Clear();
        else if (operation == "remove") cart.RemoveItem(productId);
        else
        {
            if (quantity is < 1 or > 999) throw new ArgumentException("Quantidade deve estar entre 1 e 999.");
            var p = await db.Products.SingleOrDefaultAsync(x => x.Id == productId, ct) ?? throw new KeyNotFoundException("Produto indisponível.");
            var final = operation == "add" ? quantity + (cart.Items.SingleOrDefault(x => x.ProductId == productId)?.Quantity ?? 0) : quantity;
            if (final > 999 || final > p.AvailableStock) throw new CommerceConflictException("Quantidade indisponível. Atualize o carrinho.");
            if (operation == "add") cart.AddOrUpdateItem(p.Id, quantity, p.Price); else cart.SetQuantity(p.Id, quantity);
        }
        await db.SaveChangesAsync(ct); return await ViewCart(customerId, ct);
    }
    private async Task<(CartView Cart, ShippingOption Shipping, string Fingerprint)> Evaluate(int customerId, int addressId, int shippingId, CancellationToken ct)
    {
        var cart = await ViewCart(customerId, ct);
        if (cart.Items.Count == 0) throw new ArgumentException("Seu carrinho está vazio.");
        if (cart.Items.Any(x => x.Price <= 0 || x.Quantity > x.AvailableStock)) throw new CommerceConflictException("Um item ficou indisponível.");
        var address = await db.Addresses.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.Id == addressId && x.IsActive, ct)
            ?? throw new KeyNotFoundException("Endereço não encontrado.");
        var customer = await db.Customers.SingleAsync(x => x.Id == customerId, ct);
        if (string.IsNullOrWhiteSpace(customer.Email) || (int)customer.CustomerTypeId < 3) throw new ArgumentException("Complete os dados do comprador.");
        var shipping = await db.Set<ShippingOption>().SingleOrDefaultAsync(x => x.Id == shippingId && x.IsActive, ct)
            ?? throw new ArgumentException("Selecione uma entrega válida.");
        if (!address.ZipCode.StartsWith(shipping.PostalCodePrefix, StringComparison.Ordinal)) throw new ArgumentException("Entrega não atende este CEP.");
        var fingerprint = Hash(JsonSerializer.Serialize(new {
            cart.Id, cart.Version, Address = new { address.Street, address.Number, address.Neighborhood, address.City, address.State, address.ZipCode },
            Shipping = new { shipping.Id, shipping.Name, shipping.Amount, shipping.PostalCodePrefix },
            Items = cart.Items.OrderBy(x => x.ProductId).Select(x => new { x.ProductId, x.Name, x.Sku, x.Quantity, x.Price })
        }));
        return (cart, shipping, fingerprint);
    }
    public async Task<CheckoutPreview> Preview(int customerId, int addressId, int shippingId, CancellationToken ct)
    {
        var evaluated = await Evaluate(customerId, addressId, shippingId, ct);
        var total = evaluated.Cart.Subtotal + evaluated.Shipping.Amount;
        var q = new Quote(db.CurrentTenantId, customerId, evaluated.Cart.Id, evaluated.Cart.Version, addressId,
            shippingId, evaluated.Fingerprint, total, DateTime.UtcNow.AddMinutes(10));
        return new(protector.Protect(JsonSerializer.Serialize(q)), evaluated.Cart, evaluated.Shipping.Name,
            evaluated.Shipping.Amount, total, q.ExpiresAt);
    }
    public async Task<PlacedOrder> Confirm(int customerId, string key, string token, CancellationToken ct)
    {
        if (!Guid.TryParse(key, out _) || token.Length > 16000) throw new ArgumentException("Chave de confirmação inválida.");
        Quote q;
        try { q = JsonSerializer.Deserialize<Quote>(protector.Unprotect(token))!; }
        catch (Exception ex) when (ex is CryptographicException or JsonException) { throw new ArgumentException("Resumo inválido. Revise a compra."); }
        if (q == null || q.TenantId != db.CurrentTenantId || q.CustomerId != customerId) throw new UnauthorizedAccessException();
        var hash = Hash(token);
        var existing = await db.Orders.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.CheckoutKey == key, ct);
        if (existing != null)
        {
            if (existing.CheckoutHash != hash) throw new CommerceConflictException("A chave já foi usada em outra confirmação.");
            return await GetOrder(customerId, existing.Id, ct);
        }
        if (q.ExpiresAt <= DateTime.UtcNow) throw new CommerceConflictException("Resumo expirado. Revise a compra.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            var e = await Evaluate(customerId, q.AddressId, q.ShippingId, ct);
            if (e.Fingerprint != q.Fingerprint || e.Cart.Subtotal + e.Shipping.Amount != q.Total)
                throw new CommerceConflictException("Preço, carrinho ou entrega mudou. Revise a compra.");
            var address = await db.Addresses.SingleAsync(x => x.Id == q.AddressId && x.CustomerId == customerId, ct);
            var products = await db.Products.Where(x => e.Cart.Items.Select(i => i.ProductId).Contains(x.Id)).ToListAsync(ct);
            foreach (var item in e.Cart.Items) products.Single(x => x.Id == item.ProductId).ReserveStock(item.Quantity);
            var order = Order.Checkout(db.CurrentTenantId, customerId,
                e.Cart.Items.Select(x => (x.ProductId, x.Quantity, x.Price)), address);
            order.ConfigureCheckout(key, hash, e.Shipping.Amount, e.Shipping.Name,
                DateTime.UtcNow.AddMinutes(Math.Clamp(config.GetValue<int?>("Checkout:ReservationMinutes") ?? 30, 5, 1440)));
            foreach (var item in order.OrderItems)
            {
                var p = products.Single(x => x.Id == item.ProductId); item.SetSnapshot(p.Name, p.Sku);
            }
            db.Add(order);
            var cart = await GetCart(customerId, ct); cart.Clear();
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
            return await GetOrder(customerId, order.Id, ct);
        }
        catch (Exception ex) when (ex is DbUpdateException)
        {
            await tx.RollbackAsync(ct); db.ChangeTracker.Clear();
            var replay = await db.Orders.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.CheckoutKey == key, ct);
            if (replay?.CheckoutHash == hash) return await GetOrder(customerId, replay.Id, ct);
            throw new CommerceConflictException("O estoque ou carrinho mudou. Atualize antes de confirmar.");
        }
    }
    public async Task<PlacedOrder> GetOrder(int customerId, int id, CancellationToken ct)
    {
        var order = await db.Orders.Include(x => x.OrderItems).Include(x => x.OrderAddress)
            .SingleOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct) ?? throw new KeyNotFoundException("Pedido não encontrado.");
        return ToView(order);
    }
    public static PlacedOrder ToView(Order order) => new(order.Id, order.TotalAmount, order.ShippingAmount,
        order.ShippingMethod, order.OrderStatusId, order.ReservationState, order.ReservationExpiresAt,
        order.OrderItems.Select(x => (object)new { x.ProductId, name = x.ProductName, x.Sku, x.Quantity, x.UnitPrice }).ToArray(),
        order.OrderAddress == null ? null : new { order.OrderAddress.Street, order.OrderAddress.Number,
            order.OrderAddress.Neighborhood, order.OrderAddress.City, order.OrderAddress.State, order.OrderAddress.ZipCode });
    public async Task Release(int orderId, int? customerId, CancellationToken ct, bool paymentResolved = false)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var order = await db.Orders.Include(x => x.OrderItems).SingleOrDefaultAsync(x => x.Id == orderId &&
            (customerId == null || x.CustomerId == customerId), ct) ?? throw new KeyNotFoundException();
        if (order.ReservationState == "Released") return;
        if (!paymentResolved && await db.Set<CloudShopping.Infrastructure.Payments.PaymentAttempt>().AnyAsync(x=>x.OrderId==orderId,ct))
            throw new CommerceConflictException("Este pedido possui integração financeira. Solicite o cancelamento pela área de pagamento.");
        if (order.OrderStatusId != 1 || order.ReservationState != "Reserved") throw new CommerceConflictException("Pedido não pode ser cancelado nesta etapa.");
        foreach (var item in order.OrderItems)
        {
            var product = await db.Products.IgnoreQueryFilters().SingleAsync(x => x.Id == item.ProductId && x.TenantId == db.CurrentTenantId, ct);
            product.ReleaseReservedStock(item.Quantity);
        }
        order.ReleaseReservation();
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
