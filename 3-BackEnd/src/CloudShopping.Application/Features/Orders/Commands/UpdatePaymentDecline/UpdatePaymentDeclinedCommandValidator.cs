using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.UpdatePaymentDeclined
{
    public sealed class UpdatePaymentDeclinedCommandValidator : AbstractValidator<UpdatePaymentDeclinedCommand>
    {
        public UpdatePaymentDeclinedCommandValidator()
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