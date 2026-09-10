using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProducts;
public sealed class GetStoreProductsQueryValidator : AbstractValidator<GetStoreProductsQuery>
{
    public GetStoreProductsQueryValidator() { RuleFor(x => x.Page).InclusiveBetween(1, int.MaxValue / 100); RuleFor(x => x.PageSize).InclusiveBetween(1, 100); }
}
