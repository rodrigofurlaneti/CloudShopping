using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreCart;
public sealed class GetStoreCartQueryValidator : AbstractValidator<GetStoreCartQuery>
{
    public GetStoreCartQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); }
}
