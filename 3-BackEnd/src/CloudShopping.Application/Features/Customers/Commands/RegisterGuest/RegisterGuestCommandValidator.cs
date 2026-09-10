using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands.RegisterGuest;
public sealed class RegisterGuestCommandValidator : AbstractValidator<RegisterGuestCommand>
{
    public RegisterGuestCommandValidator()
    {
        // Parameterless command; tenant and authorization are resolved by the use case.
        RuleFor(x=>x).NotNull();
    }
}
