using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class DirectCheckoutCommandValidator : AbstractValidator<DirectCheckoutCommand>
    {
        public DirectCheckoutCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("O ID do cliente é inválido.");
            RuleFor(x => x.DeliveryAddress).NotNull().WithMessage("O endereço de entrega é obrigatório.");
            RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve conter ao menos um item.");
            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
            RuleFor(x => x.DeliveryAddress).SetValidator(new AddressDtoValidator()!);
        }
    }
}
