using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.UpdateProductLocation
{
    public sealed class UpdateProductLocationCommandValidator : AbstractValidator<UpdateProductLocationCommand>
    {
        public UpdateProductLocationCommandValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("O ID do produto é inválido.");
            RuleFor(x => x.Aisle).NotEmpty().WithMessage("O corredor é obrigatório.");
            RuleFor(x => x.Rack).NotEmpty().WithMessage("A estante é obrigatória.");
            RuleFor(x => x.Level).NotEmpty().WithMessage("O nível é obrigatório.");
            RuleFor(x => x.Position).NotEmpty().WithMessage("A posição é obrigatória.");
        }
    }
}
