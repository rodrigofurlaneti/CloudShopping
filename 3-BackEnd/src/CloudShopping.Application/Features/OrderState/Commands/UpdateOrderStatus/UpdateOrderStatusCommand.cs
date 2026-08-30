using MediatR;
namespace CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus
{
    public sealed record UpdateOrderStatusCommand(
            int Id,
            int OrderSectorId,
            string Name
        ) : IRequest; // Retorna Unit (void)
}
