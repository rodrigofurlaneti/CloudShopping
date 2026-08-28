using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class RegisterB2CCommandValidator : AbstractValidator<RegisterB2CCommand>
    {
        public RegisterB2CCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("ID do cliente é obrigatório.");
            RuleFor(x => x.TaxId).Length(11).WithMessage("O CPF deve conter exatamente 11 caracteres.");
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100).WithMessage("Nome completo inválido.");
        }
    }
}
