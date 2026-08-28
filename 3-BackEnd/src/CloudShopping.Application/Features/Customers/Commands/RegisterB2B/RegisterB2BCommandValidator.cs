using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands.RegisterB2B
{
    public sealed class RegisterB2BCommandValidator : AbstractValidator<RegisterB2BCommand>
    {
        public RegisterB2BCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("ID do cliente é obrigatório.");
            RuleFor(x => x.BusinessTaxId)
                .NotEmpty()
                .Length(14).WithMessage("O CNPJ deve conter exatamente 14 caracteres numéricos.");
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("A Razão Social é obrigatória.")
                .MaximumLength(150).WithMessage("A Razão Social deve ter no máximo 150 caracteres.");
            RuleFor(x => x.StateTaxId)
                .MaximumLength(15).WithMessage("A Inscrição Estadual deve ter no máximo 15 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.StateTaxId));
        }
    }
}
