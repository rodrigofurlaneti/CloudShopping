using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.AddPendingPayment
{
    public sealed class AddPendingPaymentCommandValidator : AbstractValidator<AddPendingPaymentCommand>
    {
        public AddPendingPaymentCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
