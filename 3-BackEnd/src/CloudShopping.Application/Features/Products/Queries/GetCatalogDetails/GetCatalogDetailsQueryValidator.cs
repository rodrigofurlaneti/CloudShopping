using FluentValidation;
namespace CloudShopping.Application.Features.Products.Queries.GetCatalogDetails;
public sealed class GetCatalogDetailsQueryValidator : AbstractValidator<GetCatalogDetailsQuery>
{
    public GetCatalogDetailsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, int.MaxValue / 20);
    }
}
