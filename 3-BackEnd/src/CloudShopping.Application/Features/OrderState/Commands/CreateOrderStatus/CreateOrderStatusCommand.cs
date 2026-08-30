using MediatR;

namespace CloudShopping.Application.OrderStatus.Commands.CreateOrderStatus
{
    public sealed record CreateOrderStatusCommand(
        int OrderSectorId,
        String Name
    ) : IRequest<int>; // Retorna o ID do novo status criado
}