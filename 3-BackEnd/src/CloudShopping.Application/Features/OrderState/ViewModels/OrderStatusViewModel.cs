namespace CloudShopping.Application.Features.OrderState.ViewModels
{
    public sealed record OrderStatusViewModel(
        int Id,
        int OrderSectorId,
        string Name,
        bool IsSystemDefault,
        bool IsActive
    );
}
