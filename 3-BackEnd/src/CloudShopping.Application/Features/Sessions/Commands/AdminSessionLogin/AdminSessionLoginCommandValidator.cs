using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Commands.AdminSessionLogin;
public sealed class AdminSessionLoginCommandValidator : AbstractValidator<AdminSessionLoginCommand>
{
    public AdminSessionLoginCommandValidator() { RuleFor(x=>x.Caller).NotNull(); RuleFor(x=>x.Username).NotEmpty().MaximumLength(100); RuleFor(x=>x.Password).NotEmpty().MaximumLength(72); }
}
