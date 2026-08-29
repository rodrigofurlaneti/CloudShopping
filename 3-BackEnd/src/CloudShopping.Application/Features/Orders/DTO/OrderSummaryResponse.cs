using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record OrderSummaryResponse(
        int Id,
        int CustomerId,
        DateTime OrderDate,
        decimal TotalAmount,
        OrderStatus OrderStatus,
        int TotalItems
    );
}
