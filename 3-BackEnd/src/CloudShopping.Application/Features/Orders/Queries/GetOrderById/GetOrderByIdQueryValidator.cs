using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("O ID do cliente é inválido.");
        }
    }
}