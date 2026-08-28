using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class AddCustomerAddressCommandValidator : AbstractValidator<AddCustomerAddressCommand>
    {
        public AddCustomerAddressCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.ZipCode).NotEmpty().Length(8).WithMessage("O CEP deve ter 8 caracteres.");
            RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("O Estado deve ter 2 caracteres (UF).");
            RuleFor(x => x.Street).NotEmpty().MaximumLength(150);
        }
    }
}
