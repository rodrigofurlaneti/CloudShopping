using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.UpdateStoreProfile;
public sealed class UpdateStoreProfileCommandValidator : AbstractValidator<UpdateStoreProfileCommand>
{
    public UpdateStoreProfileCommandValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100); RuleFor(x => x.Name).NotEmpty().MaximumLength(150); RuleFor(x => x.Type).Must(x => x == "B2C" || x == "B2B"); RuleFor(x => x.TaxId).NotEmpty(); }
}
