using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands.ChangeCustomerEmail
{
    public sealed class ChangeCustomerEmailCommandValidator : AbstractValidator<ChangeCustomerEmailCommand>
    {
        public ChangeCustomerEmailCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("O novo e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail é inválido.")
                .MaximumLength(100).WithMessage("O e-mail deve ter no máximo 100 caracteres.");
        }
    }
}
