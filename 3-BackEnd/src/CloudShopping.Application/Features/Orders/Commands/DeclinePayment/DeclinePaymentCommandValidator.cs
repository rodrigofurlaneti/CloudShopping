using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DeclinePayment
{
    public sealed class DeclinePaymentCommandValidator : AbstractValidator<DeclinePaymentCommand>
    {
        public DeclinePaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");

            RuleFor(x => x.PaymentId)
                .GreaterThan(0)
                .WithMessage("O ID do pagamento é inválido.");
        }
    }
}
