using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.UpdateProfile
{
    public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do perfil informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do perfil é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O nome do perfil não pode exceder 100 caracteres.");
        }
    }
}