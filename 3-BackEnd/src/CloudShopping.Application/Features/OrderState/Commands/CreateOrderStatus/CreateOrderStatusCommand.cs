using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus
{
    public sealed record CreateOrderStatusCommand(
        int OrderSectorId,
        string Name
    ) : IRequest<Result<int>>; // Retorna o ID do novo status criado
}
