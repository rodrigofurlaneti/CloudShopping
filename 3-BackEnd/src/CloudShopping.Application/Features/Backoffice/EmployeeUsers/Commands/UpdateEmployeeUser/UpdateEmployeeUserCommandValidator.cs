using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.UpdateEmployeeUser
{
    public sealed class UpdateEmployeeUserCommandValidator : AbstractValidator<UpdateEmployeeUserCommand>
    {
        public UpdateEmployeeUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do usuário do backoffice informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("O nome de usuário (username) é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O nome de usuário não pode ter mais de 100 caracteres.");

            RuleFor(x => x.NewPassword)
                .MinimumLength(6)
                .When(x => !string.IsNullOrEmpty(x.NewPassword))
                .WithMessage("A nova senha deve ter pelo menos 6 caracteres.");
        }
    }
}