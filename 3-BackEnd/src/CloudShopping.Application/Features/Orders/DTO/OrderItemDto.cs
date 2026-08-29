namespace CloudShopping.Application.Features.Orders.DTO
{
    public record OrderItemDto(int ProductId, int Quantity, decimal UnitPrice);
}
