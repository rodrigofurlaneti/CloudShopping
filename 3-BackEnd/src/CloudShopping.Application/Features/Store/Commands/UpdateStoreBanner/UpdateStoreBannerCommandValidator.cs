using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Store.Commands.UpdateStoreBanner
{
    public sealed class UpdateStoreBannerCommandValidator : AbstractValidator<UpdateStoreBannerCommand>
    {
        public UpdateStoreBannerCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID do banner inválido.");

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
