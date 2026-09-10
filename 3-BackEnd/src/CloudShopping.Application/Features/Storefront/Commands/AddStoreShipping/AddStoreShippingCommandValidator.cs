using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.AddStoreShipping;
public sealed class AddStoreShippingCommandValidator : AbstractValidator<AddStoreShippingCommand>
{
    public AddStoreShippingCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(100); RuleFor(x => x.Amount).InclusiveBetween(0, 999999.99m); RuleFor(x => x.EstimatedDays).InclusiveBetween(0,365); RuleFor(x => x.PostalCodePrefix).NotNull().Matches("^[0-9]{0,8}$"); }
}
