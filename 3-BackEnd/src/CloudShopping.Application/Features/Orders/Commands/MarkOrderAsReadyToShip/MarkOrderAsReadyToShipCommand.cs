using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsReadyToShip
{
    public sealed record MarkOrderAsReadyToShipCommand(int OrderId, int TenantId) : IRequest<Result>;
}
