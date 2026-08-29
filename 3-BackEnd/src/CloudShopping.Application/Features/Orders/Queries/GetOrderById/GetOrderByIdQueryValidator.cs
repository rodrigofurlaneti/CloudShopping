using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.CustomerId).GreaterThan(0);
        }
    }
}
