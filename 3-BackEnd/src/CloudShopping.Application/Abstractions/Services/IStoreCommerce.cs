using CloudShopping.Application.Features.Storefront.Contracts;
namespace CloudShopping.Application.Abstractions.Services;
public interface IStoreCommerce
{
    Task<CartView> ViewCart(int customerId, CancellationToken ct);
    Task<CartView> ChangeCart(int customerId, int productId, int quantity, string operation, CancellationToken ct);
    Task<CheckoutPreview> Preview(int customerId, int addressId, int shippingId, CancellationToken ct, string? couponCode = null);
    Task<PlacedOrder> Confirm(int customerId, string key, string token, CancellationToken ct);
    Task<PlacedOrder> GetOrder(int customerId, int id, CancellationToken ct);
}
public interface ICustomerPaymentCancellation
{
    Task Cancel(int orderId, int customerId, CancellationToken ct);
}
