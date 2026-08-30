using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.StartOrderPacking
{
    public sealed class StartOrderPackingCommandValidator : AbstractValidator<StartOrderPackingCommand>
    {
        public StartOrderPackingCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}