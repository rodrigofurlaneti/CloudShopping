using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.DeleteEmployeeUser
{
    public sealed class DeleteEmployeeUserCommandValidator : AbstractValidator<DeleteEmployeeUserCommand>
    {
        public DeleteEmployeeUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do usuário do backoffice informado é inválido.");
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}