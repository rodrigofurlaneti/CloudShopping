using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.Queries
{
    public sealed record OrderDetailsResponse(
        int Id,
        int CustomerId,
        DateTime OrderDate,
        decimal TotalAmount,
        OrderStatus OrderStatus,
        OrderAddressResponse? Address,
        IReadOnlyCollection<OrderItemResponse> Items,
        IReadOnlyCollection<PaymentResponse> Payments
    );
}
