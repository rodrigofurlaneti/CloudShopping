using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record OrderDetailsResponse(
        int Id,
        int CustomerId,
        DateTime OrderDate,
        decimal TotalAmount,
        OrderStatusEnum OrderStatus,
        OrderAddressResponse? Address,
        IReadOnlyCollection<OrderItemResponse> Items,
        IReadOnlyCollection<PaymentResponse> Payments
    );
}
