using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline
{
    public sealed class GetOrderTimelineQueryValidator : AbstractValidator<GetOrderTimelineQuery>
    {
        public GetOrderTimelineQueryValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("O ID do cliente é inválido.");
        }
    }
}
