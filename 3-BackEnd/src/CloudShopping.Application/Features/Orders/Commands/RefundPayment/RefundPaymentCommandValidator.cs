using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.RefundPayment
{
    public sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
    {
        public RefundPaymentCommandValidator()
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
