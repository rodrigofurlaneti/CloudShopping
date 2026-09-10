using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Commands.CustomerSessionLogin;
public sealed class CustomerSessionLoginCommandValidator : AbstractValidator<CustomerSessionLoginCommand>
{
    public CustomerSessionLoginCommandValidator() { RuleFor(x=>x.Caller).NotNull(); RuleFor(x=>x.Username).NotEmpty().MaximumLength(100); RuleFor(x=>x.Password).NotEmpty().MaximumLength(72); }
}
