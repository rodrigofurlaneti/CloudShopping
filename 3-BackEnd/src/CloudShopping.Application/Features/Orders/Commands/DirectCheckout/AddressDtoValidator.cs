using CloudShopping.Application.Features.Orders.DTO;
using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(x => x.Street).NotEmpty().MaximumLength(150).WithMessage("A rua é obrigatória e deve ter até 150 caracteres.");
            RuleFor(x => x.Number).NotEmpty().MaximumLength(10).WithMessage("O número é obrigatório.");
            RuleFor(x => x.City).NotEmpty().MaximumLength(50).WithMessage("A cidade é obrigatória.");
            RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("O estado deve conter exatamente 2 caracteres (UF).");
            RuleFor(x => x.ZipCode).NotEmpty().Length(8).WithMessage("O CEP deve conter 8 caracteres.");
        }
    }
}
