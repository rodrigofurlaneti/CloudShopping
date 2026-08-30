using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record OrderSummaryResponse(
            int OrderId,
            int CustomerId,
            DateTime OrderDate,
            decimal TotalAmount,
            string StatusName);
}
