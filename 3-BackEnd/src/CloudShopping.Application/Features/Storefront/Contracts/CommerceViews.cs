namespace CloudShopping.Application.Features.Storefront.Contracts;
public sealed record CartLine(int ProductId, string Name, string Sku, decimal Price, int Quantity, int AvailableStock, string? Image);
public sealed record CartView(int Id, int Version, DateTime ExpiresAt, IReadOnlyList<CartLine> Items, decimal Subtotal);
public sealed record Quote(int TenantId, int CustomerId, int CartId, int CartVersion, int AddressId, int ShippingId,
    string Fingerprint, decimal Total, DateTime ExpiresAt,string? CouponCode=null,decimal Discount=0,string CouponFingerprint="");
public sealed record CheckoutPreview(string Token, CartView Cart, string ShippingName, decimal ShippingAmount, decimal Total, DateTime ExpiresAt,decimal DiscountAmount=0,string? CouponCode=null);
public sealed record PlacedOrder(int Id, decimal TotalAmount, decimal ShippingAmount, string? ShippingMethod,
    int OrderStatusId, string ReservationState, DateTime? ReservationExpiresAt, object[] Items, object? Address,decimal DiscountAmount=0,string? CouponCode=null);

