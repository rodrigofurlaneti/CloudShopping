using FluentValidation;

namespace CloudShopping.Application.Features.Carts.Commands
{
    public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
    {
        public AddCartItemCommandValidator()
        {
            RuleFor(x => x.CartId)
                .GreaterThan(0)
                .WithMessage("O identificador do carrinho é inválido.");

            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("O identificador do produto é inválido.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("A quantidade deve ser maior que zero.");
        }
    }
}