using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.OrderSector.Commands.UpdateOrderSector
{
    public sealed record UpdateOrderSectorNameCommand(int Id, string NewName) : IRequest<Result>;
}
