using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Commands.RegisterSessionAccount;
public sealed class RegisterSessionAccountCommandValidator : AbstractValidator<RegisterSessionAccountCommand>
{
    public RegisterSessionAccountCommandValidator() { RuleFor(x=>x.Caller).NotNull(); RuleFor(x=>x.Email).NotEmpty().EmailAddress().MaximumLength(100); RuleFor(x=>x.Password).NotEmpty().MinimumLength(12).Must(x=>x!=null&&System.Text.Encoding.UTF8.GetByteCount(x)<=72); }
}
