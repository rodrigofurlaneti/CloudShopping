namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record OrderItemResponse(int ProductId, int Quantity, decimal UnitPrice);
}
