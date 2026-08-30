using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.OrderSector.Commands.ToggleOrderSectorStatus
{
    public sealed record ToggleOrderSectorStatusCommand(int Id, bool Activate) : IRequest<Result>;
}
