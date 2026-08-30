using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.ApprovePayment
{
    public sealed class ApprovePaymentCommandValidator : AbstractValidator<ApprovePaymentCommand>
    {
        public ApprovePaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.PaymentId)
                .GreaterThan(0).WithMessage("O ID do pagamento é inválido.");
        }
    }
}