using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.ConfirmStoreCheckout;
public sealed class ConfirmStoreCheckoutCommandValidator : AbstractValidator<ConfirmStoreCheckoutCommand>
{
    public ConfirmStoreCheckoutCommandValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Key).NotEmpty().MaximumLength(64); RuleFor(x => x.Token).NotEmpty().MaximumLength(16000); }
}
