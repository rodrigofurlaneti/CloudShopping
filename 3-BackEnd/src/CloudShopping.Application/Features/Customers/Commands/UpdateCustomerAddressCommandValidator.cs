using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class UpdateCustomerAddressCommandValidator : AbstractValidator<UpdateCustomerAddressCommand>
    {
        public UpdateCustomerAddressCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.AddressId).GreaterThan(0);
            RuleFor(x => x.ZipCode).NotEmpty().Length(8).WithMessage("O CEP deve conter 8 dígitos.");
            RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("O Estado (UF) deve conter 2 letras.");
            RuleFor(x => x.Street).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Number).NotEmpty().MaximumLength(10);
        }
    }
}
