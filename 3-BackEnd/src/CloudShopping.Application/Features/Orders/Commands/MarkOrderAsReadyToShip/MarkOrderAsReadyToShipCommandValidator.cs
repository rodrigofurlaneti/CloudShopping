using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsReadyToShip
{
    public sealed class MarkOrderAsReadyToShipCommandValidator : AbstractValidator<MarkOrderAsReadyToShipCommand>
    {
        public MarkOrderAsReadyToShipCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}