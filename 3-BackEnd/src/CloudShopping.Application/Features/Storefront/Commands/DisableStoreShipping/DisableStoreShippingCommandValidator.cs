using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.DisableStoreShipping;
public sealed class DisableStoreShippingCommandValidator : AbstractValidator<DisableStoreShippingCommand>
{
    public DisableStoreShippingCommandValidator() { RuleFor(x => x.Id).GreaterThan(0); }
}
