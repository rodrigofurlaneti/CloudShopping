using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.UpdateProfileUser
{
    public sealed class UpdateProfileUserCommandValidator : AbstractValidator<UpdateProfileUserCommand>
    {
        public UpdateProfileUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do vínculo de perfil do usuário informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}