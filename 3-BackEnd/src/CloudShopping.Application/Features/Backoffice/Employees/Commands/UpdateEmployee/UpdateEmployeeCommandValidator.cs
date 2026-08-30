using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.UpdateEmployee
{
    public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
    {
        public UpdateEmployeeCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do funcionário informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do funcionário é obrigatório.")
                .MaximumLength(150)
                .WithMessage("O nome não pode exceder 150 caracteres.");

            RuleFor(x => x.Cpf)
                .NotEmpty()
                .WithMessage("O CPF é obrigatório.")
                .Length(11)
                .WithMessage("O CPF deve conter exatamente 11 caracteres.")
                .Matches(@"^\d{11}$")
                .WithMessage("O CPF deve conter apenas números.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("O formato do e-mail é inválido.")
                .MaximumLength(150)
                .WithMessage("O e-mail não pode exceder 150 caracteres.");

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("O telefone não pode exceder 20 caracteres.");

            RuleFor(x => x.HiredAt)
                .NotEmpty()
                .WithMessage("A data de contratação é obrigatória.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Salary.HasValue)
                .WithMessage("O salário não pode ser negativo.");

            RuleFor(x => x.CommissionPercent)
                .InclusiveBetween(0, 100)
                .When(x => x.CommissionPercent.HasValue)
                .WithMessage("O percentual de comissão deve estar entre 0 e 100.");
        }
    }
}