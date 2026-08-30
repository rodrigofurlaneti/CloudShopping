using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.DeleteProfile
{
    public sealed class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommand>
    {
        public DeleteProfileCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do perfil informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}