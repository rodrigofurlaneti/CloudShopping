using FluentValidation;
namespace CloudShopping.Application.Features.Store.Commands.CreateStoreBanner
{
    public sealed class CreateStoreBannerCommandValidator : AbstractValidator<CreateStoreBannerCommand>
    {
        public CreateStoreBannerCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("O título do banner é obrigatório.")
                .MaximumLength(150).WithMessage("O título pode ter no máximo 150 caracteres.");

            RuleFor(x => x.Subtitle)
                .MaximumLength(250).WithMessage("O subtítulo pode ter no máximo 250 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Subtitle));

            RuleFor(x => x.ButtonText)
                .NotEmpty().WithMessage("O texto do botão é obrigatório.")
                .MaximumLength(50).WithMessage("O texto do botão pode ter no máximo 50 caracteres.");

            RuleFor(x => x.ButtonLink)
                .NotEmpty().WithMessage("O link do botão é obrigatório.")
                .MaximumLength(250).WithMessage("O link pode ter no máximo 250 caracteres.");

            RuleFor(x => x.BackgroundColor)
                .NotEmpty().WithMessage("A cor de fundo é obrigatória.")
                .MaximumLength(30).WithMessage("A cor de fundo pode ter no máximo 30 caracteres.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThan(0).WithMessage("A ordem de exibição deve ser maior que zero.");
        }
    }
}
