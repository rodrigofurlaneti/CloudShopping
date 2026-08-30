using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("O ID do cliente é inválido.");
        }
    }
}