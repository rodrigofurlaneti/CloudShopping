using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class RegisterLeadCommandValidator : AbstractValidator<RegisterLeadCommand>
    {
        public RegisterLeadCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("ID do cliente é obrigatório.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail é inválido.")
                .MaximumLength(100).WithMessage("O e-mail deve ter no máximo 100 caracteres.");
        }
    }
}
