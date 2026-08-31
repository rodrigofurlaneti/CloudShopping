using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.OrderState.Commands.ToggleOrderStatusStatus
{
    public sealed record ToggleOrderStatusStatusCommand(int Id, bool Activate) : IRequest<Result>;
}
