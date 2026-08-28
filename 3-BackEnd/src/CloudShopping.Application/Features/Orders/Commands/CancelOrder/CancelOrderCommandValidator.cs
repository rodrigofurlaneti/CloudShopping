using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
        }
    }
}
