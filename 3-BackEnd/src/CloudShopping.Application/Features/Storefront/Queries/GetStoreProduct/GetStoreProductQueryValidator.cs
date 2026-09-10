using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProduct;
public sealed class GetStoreProductQueryValidator : AbstractValidator<GetStoreProductQuery>
{
    public GetStoreProductQueryValidator() { RuleFor(x => x).Must(x => x.Id > 0 || !string.IsNullOrWhiteSpace(x.Slug)); }
}
