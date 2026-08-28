using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class MarkOrderAsPaidCommandValidator : AbstractValidator<MarkOrderAsPaidCommand>
    {
        public MarkOrderAsPaidCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
