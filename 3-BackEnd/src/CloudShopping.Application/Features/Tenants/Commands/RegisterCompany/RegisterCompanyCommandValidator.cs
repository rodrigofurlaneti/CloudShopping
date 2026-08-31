using FluentValidation;

namespace CloudShopping.Application.Features.Tenants.Commands.RegisterCompany
{
    public sealed class RegisterCompanyCommandValidator : AbstractValidator<RegisterCompanyCommand>
    {
        public RegisterCompanyCommandValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
                .MaximumLength(150);

            RuleFor(x => x.Domain)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Domain));

            RuleFor(x => x.AdminName)
                .NotEmpty().WithMessage("O nome do administrador é obrigatório.")
                .MaximumLength(150);

            RuleFor(x => x.AdminCpf)
                .NotEmpty().WithMessage("O CPF do administrador é obrigatório.")
                .Matches(@"^\d{11}$").WithMessage("O CPF deve conter exatamente 11 números, sem pontos ou traços.");

            RuleFor(x => x.AdminEmail)
                .NotEmpty().WithMessage("O e-mail do administrador é obrigatório.")
                .EmailAddress().WithMessage("Informe um e-mail válido.")
                .MaximumLength(150);

            RuleFor(x => x.AdminPhone)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.AdminPhone));

            RuleFor(x => x.AdminUsername)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
                .MaximumLength(100)
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("O usuário deve conter apenas letras, números, ponto, hífen ou underline.");

            RuleFor(x => x.AdminPassword)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.");
        }
    }
}
