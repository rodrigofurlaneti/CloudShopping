using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Commands.CreateGuestSession;
public sealed class CreateGuestSessionCommandValidator : AbstractValidator<CreateGuestSessionCommand>
{
    public CreateGuestSessionCommandValidator() { RuleFor(x=>x.Caller).NotNull(); }
}
