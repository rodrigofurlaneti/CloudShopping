using FluentValidation;

namespace CloudShopping.Application.Features.Store.Commands.DeleteStoreBanner
{
    public sealed class DeleteStoreBannerCommandValidator : AbstractValidator<DeleteStoreBannerCommand>
    {
        public DeleteStoreBannerCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("ID do banner inválido.");
        }
    }
}