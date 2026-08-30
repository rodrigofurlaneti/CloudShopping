using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.StartOrderProcessing
{
    public sealed class StartOrderProcessingCommandValidator : AbstractValidator<StartOrderProcessingCommand>
    {
        public StartOrderProcessingCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}