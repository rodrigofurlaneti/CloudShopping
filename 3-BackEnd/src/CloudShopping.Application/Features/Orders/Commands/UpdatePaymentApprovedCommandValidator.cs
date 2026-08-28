using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class UpdatePaymentApprovedCommandValidator : AbstractValidator<UpdatePaymentApprovedCommand>
    {
        public UpdatePaymentApprovedCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.PaymentId).GreaterThan(0);
        }
    }
}
