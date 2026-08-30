using MediatR;

namespace CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus
{
    public sealed record CreateOrderStatusCommand(
        int OrderSectorId,
        string Name
    ) : IRequest<int>; // Retorna o ID do novo status criado
}
