using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.Queries
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
