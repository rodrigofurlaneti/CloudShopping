using CloudShopping.Application.Features.Orders.DTO;
using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitPrice).GreaterThan(0);
        }
    }
}
