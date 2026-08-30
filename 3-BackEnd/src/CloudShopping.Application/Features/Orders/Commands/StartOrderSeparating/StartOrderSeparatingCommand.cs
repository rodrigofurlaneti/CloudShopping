using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.StartOrderSeparating
{
    public sealed record StartOrderSeparatingCommand(int OrderId) : IRequest<Result>;
}
