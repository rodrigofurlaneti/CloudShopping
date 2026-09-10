using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.AddStoreAddress;
public sealed class AddStoreAddressCommandValidator : AbstractValidator<AddStoreAddressCommand>
{
    public AddStoreAddressCommandValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Street).NotEmpty().MaximumLength(150); RuleFor(x => x.Number).NotEmpty().MaximumLength(10); RuleFor(x => x.Neighborhood).MaximumLength(50); RuleFor(x => x.City).NotEmpty().MaximumLength(50); RuleFor(x => x.State).Matches("^[A-Za-z]{2}$"); RuleFor(x => x.ZipCode).Matches("^[0-9]{8}$"); }
}
