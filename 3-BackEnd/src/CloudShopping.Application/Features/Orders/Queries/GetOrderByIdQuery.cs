using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Queries
{
    public sealed record GetOrderByIdQuery(int OrderId) : IRequest<Result<OrderDetailsResponse>>;
}
