using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed record CancelOrderCommand(int OrderId) : IRequest<Result>;
}
