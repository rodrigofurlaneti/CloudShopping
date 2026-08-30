using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.AddPendingPayment
{
    public sealed class AddPendingPaymentCommandValidator : AbstractValidator<AddPendingPaymentCommand>
    {
        public AddPendingPaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("O método de pagamento é obrigatório.")
                .MaximumLength(50).WithMessage("O método de pagamento deve ter no máximo 50 caracteres.");
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor do pagamento deve ser maior que zero.");
        }
    }
}