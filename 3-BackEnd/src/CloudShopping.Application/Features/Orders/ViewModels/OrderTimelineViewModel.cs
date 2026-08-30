namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public record OrderTimelineViewModel(DateTime Date, string StatusName, string? Notes);
}
