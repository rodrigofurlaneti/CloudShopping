namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public record OrderSummaryViewModel(int OrderId, DateTime OrderDate, decimal TotalAmount, string StatusName);
}
