using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.UpdatePaymentRefunded
{
    public sealed class UpdatePaymentRefundedCommandValidator : AbstractValidator<UpdatePaymentRefundedCommand>
    {
        public UpdatePaymentRefundedCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.PaymentId).GreaterThan(0);
        }
    }
}
