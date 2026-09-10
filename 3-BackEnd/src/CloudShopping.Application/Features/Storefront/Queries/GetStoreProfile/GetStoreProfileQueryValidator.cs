using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProfile;
public sealed class GetStoreProfileQueryValidator : AbstractValidator<GetStoreProfileQuery>
{
    public GetStoreProfileQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); }
}
