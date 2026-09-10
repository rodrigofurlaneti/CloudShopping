using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreShipping;
public sealed class GetStoreShippingQueryValidator : AbstractValidator<GetStoreShippingQuery>
{
    public GetStoreShippingQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.AddressId).GreaterThan(0); }
}
