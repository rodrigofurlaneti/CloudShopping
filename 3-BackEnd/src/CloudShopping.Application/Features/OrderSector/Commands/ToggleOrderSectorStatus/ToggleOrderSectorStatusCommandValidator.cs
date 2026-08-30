using FluentValidation;
namespace CloudShopping.Application.Features.OrderSector.Commands.ToggleOrderSectorStatus
{
    public sealed class ToggleOrderSectorStatusCommandValidator : AbstractValidator<ToggleOrderSectorStatusCommand>
    {
        public ToggleOrderSectorStatusCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
