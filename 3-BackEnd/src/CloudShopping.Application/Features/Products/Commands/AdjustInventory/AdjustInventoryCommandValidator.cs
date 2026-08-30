using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.AdjustInventory
{
    public sealed class AdjustInventoryCommandValidator : AbstractValidator<AdjustInventoryCommand>
    {
        public AdjustInventoryCommandValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("O ID do produto é inválido.");
            RuleFor(x => x.NewPhysicalQuantity).GreaterThanOrEqualTo(0).WithMessage("A quantidade física não pode ser negativa.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("O motivo do ajuste de inventário é obrigatório.");
        }
    }
}
