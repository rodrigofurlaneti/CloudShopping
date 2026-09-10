using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Commands.LogoutSession;
public sealed class LogoutSessionCommandValidator : AbstractValidator<LogoutSessionCommand>
{
    public LogoutSessionCommandValidator() { RuleFor(x=>x.Caller).NotNull(); }
}
