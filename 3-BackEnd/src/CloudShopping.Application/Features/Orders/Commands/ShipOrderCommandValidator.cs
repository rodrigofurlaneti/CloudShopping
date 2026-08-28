using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderCommandValidator() => RuleFor(x => x.OrderId).GreaterThan(0);
    }
}
