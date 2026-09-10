using FluentValidation;
namespace CloudShopping.Application.Features.Products.Commands.UpdateCatalogDetails;
public sealed class UpdateCatalogDetailsCommandValidator : AbstractValidator<UpdateCatalogDetailsCommand>
{
    public UpdateCatalogDetailsCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotNull().MaximumLength(10000);
        RuleFor(x => x.Attributes).NotNull();
    }
}
