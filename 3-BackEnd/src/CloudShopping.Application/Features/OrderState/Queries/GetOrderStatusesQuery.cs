using CloudShopping.Application.Features.OrderState.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.OrderState.Queries
{
    public sealed record GetOrderStatusesQuery(bool OnlyActive = false) : IRequest<Result<IEnumerable<OrderStatusViewModel>>>;
}
