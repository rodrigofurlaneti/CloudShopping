using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Auth.Commands.Login
{
    public sealed class LoginEmployeeCommandValidator : AbstractValidator<LoginEmployeeCommand>
    {
        public LoginEmployeeCommandValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("O nome de usuário (username) é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O nome de usuário não pode ter mais de 100 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("A senha é obrigatória.")
                .MinimumLength(6)
                .WithMessage("A senha deve ter pelo menos 6 caracteres.");
        }
    }
}