using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class DirectCheckoutItemDtoValidator : AbstractValidator<DirectCheckoutItemDto>
    {
        public DirectCheckoutItemDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Produto inválido.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        }
    }

    public sealed class DirectCheckoutAddressDtoValidator : AbstractValidator<DirectCheckoutAddressDto>
    {
        public DirectCheckoutAddressDtoValidator()
        {
            RuleFor(x => x.Street).NotEmpty().MaximumLength(150).WithMessage("A rua é obrigatória e deve ter até 150 caracteres.");
            RuleFor(x => x.Number).NotEmpty().MaximumLength(10).WithMessage("O número é obrigatório.");
            RuleFor(x => x.City).NotEmpty().MaximumLength(50).WithMessage("A cidade é obrigatória.");
            RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("O estado deve conter exatamente 2 caracteres (UF).");
            RuleFor(x => x.ZipCode).NotEmpty().Length(8).WithMessage("O CEP deve conter 8 caracteres.");
        }
    }

    public sealed class DirectCheckoutCommandValidator : AbstractValidator<DirectCheckoutCommand>
    {
        public DirectCheckoutCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("O ID do cliente é inválido.");
            RuleFor(x => x.DeliveryAddress).NotNull().WithMessage("O endereço de entrega é obrigatório.");
            RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve conter ao menos um item.");
            RuleForEach(x => x.Items).SetValidator(new DirectCheckoutItemDtoValidator());
            RuleFor(x => x.DeliveryAddress).SetValidator(new DirectCheckoutAddressDtoValidator()!);
        }
    }
}
