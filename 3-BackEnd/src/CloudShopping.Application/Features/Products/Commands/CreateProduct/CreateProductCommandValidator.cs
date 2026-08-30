using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("O SKU do produto é obrigatório.")
                .MaximumLength(50).WithMessage("O SKU deve ter no máximo 50 caracteres.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do produto é obrigatório.")
                .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("O preço do produto deve ser maior que zero.");
            RuleFor(x => x.InitialStock)
                .GreaterThanOrEqualTo(0).WithMessage("O estoque inicial não pode ser negativo.");
            When(x => !string.IsNullOrEmpty(x.Aisle) || !string.IsNullOrEmpty(x.Rack), () =>
            {
                RuleFor(x => x.Aisle).NotEmpty().WithMessage("O corredor é obrigatório se a localização for informada.");
                RuleFor(x => x.Rack).NotEmpty().WithMessage("A estante é obrigatória se a localização for informada.");
                RuleFor(x => x.Level).NotEmpty().WithMessage("O nível é obrigatório se a localização for informada.");
                RuleFor(x => x.Position).NotEmpty().WithMessage("A posição é obrigatória se a localização for informada.");
            });
        }
    }
}
