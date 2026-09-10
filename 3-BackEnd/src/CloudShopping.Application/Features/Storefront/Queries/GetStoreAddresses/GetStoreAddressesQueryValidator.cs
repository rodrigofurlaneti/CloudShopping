using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreAddresses;
public sealed class GetStoreAddressesQueryValidator : AbstractValidator<GetStoreAddressesQuery>
{
    public GetStoreAddressesQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); }
}
