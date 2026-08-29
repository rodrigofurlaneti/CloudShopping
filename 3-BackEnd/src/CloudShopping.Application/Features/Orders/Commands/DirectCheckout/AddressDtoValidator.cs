using CloudShopping.Application.Features.Orders.DTO;
using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(x => x.Street).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Number).NotEmpty().MaximumLength(10);
            RuleFor(x => x.City).NotEmpty().MaximumLength(50);
            RuleFor(x => x.State).NotEmpty().Length(2);
            RuleFor(x => x.ZipCode).NotEmpty().Length(8);
        }
    }
}
