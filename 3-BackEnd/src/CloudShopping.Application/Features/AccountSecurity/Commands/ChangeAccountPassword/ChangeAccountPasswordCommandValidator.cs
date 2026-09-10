using FluentValidation;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.ChangeAccountPassword;
public sealed class ChangeAccountPasswordCommandValidator : AbstractValidator<ChangeAccountPasswordCommand>
{
    public ChangeAccountPasswordCommandValidator() { RuleFor(x=>x.Subject).GreaterThan(0); RuleFor(x=>x.Kind).Must(x=>x is "Administrator" or "Customer"); RuleFor(x=>x.Current).NotEmpty().Length(32); RuleFor(x=>x.CurrentPassword).NotEmpty().MaximumLength(72); RuleFor(x=>x.NewPassword).NotEmpty().MinimumLength(12).Must(x=>x!=null&&System.Text.Encoding.UTF8.GetByteCount(x)<=72); }
}
