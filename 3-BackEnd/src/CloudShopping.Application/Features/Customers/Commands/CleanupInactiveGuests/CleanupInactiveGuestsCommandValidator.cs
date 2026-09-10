using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands.CleanupInactiveGuests;
public sealed class CleanupInactiveGuestsCommandValidator : AbstractValidator<CleanupInactiveGuestsCommand>
{
    public CleanupInactiveGuestsCommandValidator()
    {
        // Parameterless command; cleanup selection belongs to the use case.
        RuleFor(x=>x).NotNull();
    }
}
