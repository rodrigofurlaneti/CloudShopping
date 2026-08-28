using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.ShipOrder
{
    public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderCommandValidator() => RuleFor(x => x.OrderId).GreaterThan(0);
    }
}
