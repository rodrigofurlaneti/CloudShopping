using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed record ShipOrderCommand(int OrderId) : IRequest<Result>;
}
