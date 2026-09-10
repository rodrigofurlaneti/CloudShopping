using FluentValidation;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.RevokeAccountSessions;
public sealed class RevokeAccountSessionsCommandValidator : AbstractValidator<RevokeAccountSessionsCommand>
{
    public RevokeAccountSessionsCommandValidator() { RuleFor(x=>x.Subject).GreaterThan(0); RuleFor(x=>x.Kind).Must(x=>x is "Administrator" or "Customer"); RuleFor(x=>x.Current).NotEmpty().Length(32); RuleFor(x=>x.Target).Matches("^[a-f0-9]{32}$").When(x=>x.Target!=null); }
}
