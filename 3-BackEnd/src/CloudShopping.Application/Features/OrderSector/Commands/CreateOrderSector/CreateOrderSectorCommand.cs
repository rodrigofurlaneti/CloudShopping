using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.OrderSector.Commands.CreateOrderSector
{
    public sealed record CreateOrderSectorCommand(string Name) : IRequest<Result<int>>;
}
