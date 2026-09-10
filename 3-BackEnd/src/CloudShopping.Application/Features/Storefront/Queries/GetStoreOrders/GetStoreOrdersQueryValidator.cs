using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreOrders;
public sealed class GetStoreOrdersQueryValidator : AbstractValidator<GetStoreOrdersQuery>
{
    public GetStoreOrdersQueryValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Page).InclusiveBetween(1, int.MaxValue / 20); }
}
