using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreOrder;
public sealed class GetStoreOrderQueryValidator : AbstractValidator<GetStoreOrderQuery>
{
    public GetStoreOrderQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Id).GreaterThan(0); }
}
