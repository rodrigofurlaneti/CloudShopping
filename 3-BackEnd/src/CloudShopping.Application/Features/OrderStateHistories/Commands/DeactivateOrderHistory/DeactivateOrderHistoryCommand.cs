namespace CloudShopping.Application.Features.OrderStateHistories.Commands.DeactivateOrderHistory
{
    public sealed record DeactivateOrderHistoryCommand(int HistoryId) : IRequest<Result>;
}
