using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.CreateProfile
{
    public sealed class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
    {
        public CreateProfileCommandValidator()
        {
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