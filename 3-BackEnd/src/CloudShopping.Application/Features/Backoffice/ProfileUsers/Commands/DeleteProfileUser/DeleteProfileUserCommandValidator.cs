using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.DeleteProfileUser
{
    public sealed class DeleteProfileUserCommandValidator : AbstractValidator<DeleteProfileUserCommand>
    {
        public DeleteProfileUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do vínculo de perfil informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}