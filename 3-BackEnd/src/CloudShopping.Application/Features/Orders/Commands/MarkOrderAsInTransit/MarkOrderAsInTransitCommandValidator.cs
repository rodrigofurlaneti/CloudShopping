using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInTransit
{
    public sealed class MarkOrderAsInTransitCommandValidator : AbstractValidator<MarkOrderAsInTransitCommand>
    {
        public MarkOrderAsInTransitCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}