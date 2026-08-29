using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline
{
    public sealed record GetOrderTimelineQuery(int OrderId, int CustomerId) : IRequest<Result<IEnumerable<OrderTimelineViewModel>>>;
}
