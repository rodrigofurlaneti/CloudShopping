using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.AddProductStock
{
    public sealed class AddProductStockCommandValidator : AbstractValidator<AddProductStockCommand>
    {
        public AddProductStockCommandValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("O ID do produto é inválido.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("A quantidade a adicionar deve ser maior que zero.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("O motivo da entrada de estoque é obrigatório.");
        }
    }
}
