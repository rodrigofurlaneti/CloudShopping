using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.StartOrderPacking
{
    public sealed record StartOrderPackingCommand(int OrderId) : IRequest<Result>;
}