using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.CreateProfileUser
{
    public sealed class CreateProfileUserCommandValidator : AbstractValidator<CreateProfileUserCommand>
    {
        public CreateProfileUserCommandValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");

            RuleFor(x => x.ProfileId)
                .GreaterThan(0)
                .WithMessage("O ID do perfil informado é inválido.");

            RuleFor(x => x.EmployeeUserId)
                .GreaterThan(0)
                .WithMessage("O ID do usuário do funcionário informado é inválido.");
        }
    }
}