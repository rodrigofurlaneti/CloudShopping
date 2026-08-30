using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.UpdatePaymentApproved
{
    public sealed class UpdatePaymentApprovedCommandValidator : AbstractValidator<UpdatePaymentApprovedCommand>
    {
        public UpdatePaymentApprovedCommandValidator()
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