using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.PreviewStoreCheckout;
public sealed class PreviewStoreCheckoutQueryValidator : AbstractValidator<PreviewStoreCheckoutQuery>
{
    public PreviewStoreCheckoutQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.AddressId).GreaterThan(0); RuleFor(x => x.ShippingId).GreaterThan(0); RuleFor(x => x.CouponCode).MaximumLength(40); }
}
