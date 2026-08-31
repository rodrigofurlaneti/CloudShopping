using FluentValidation;

namespace CloudShopping.Application.Features.OrderState.Commands.ToggleOrderStatusStatus
{
    public sealed class ToggleOrderStatusStatusCommandValidator : AbstractValidator<ToggleOrderStatusStatusCommand>
    {
        public ToggleOrderStatusStatusCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
