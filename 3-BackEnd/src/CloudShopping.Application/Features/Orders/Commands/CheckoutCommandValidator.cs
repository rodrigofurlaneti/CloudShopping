using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
    {
        public CheckoutCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("ID do cliente inválido.");
            RuleFor(x => x.CartId).GreaterThan(0).WithMessage("ID do carrinho inválido.");
            RuleFor(x => x.AddressId).GreaterThan(0).WithMessage("ID do endereço inválido.");
        }
    }
}
