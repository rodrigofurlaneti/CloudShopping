namespace CloudShopping.Application.Features.Orders.Queries
{
    public sealed record OrderItemResponse(int ProductId, int Quantity, decimal UnitPrice);
}
