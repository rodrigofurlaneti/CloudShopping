using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class UpdatePaymentDeclinedCommandValidator : AbstractValidator<UpdatePaymentDeclinedCommand>
    {
        public UpdatePaymentDeclinedCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.PaymentId).GreaterThan(0);
        }
    }
}
