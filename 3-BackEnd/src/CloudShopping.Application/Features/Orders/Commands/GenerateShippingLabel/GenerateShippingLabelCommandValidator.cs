using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.GenerateShippingLabel
{
    public sealed class GenerateShippingLabelCommandValidator : AbstractValidator<GenerateShippingLabelCommand>
    {
        public GenerateShippingLabelCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}