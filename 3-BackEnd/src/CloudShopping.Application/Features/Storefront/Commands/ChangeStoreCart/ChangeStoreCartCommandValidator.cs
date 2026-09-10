using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.ChangeStoreCart;
public sealed class ChangeStoreCartCommandValidator : AbstractValidator<ChangeStoreCartCommand>
{
    public ChangeStoreCartCommandValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Operation).Must(x => new[]{"add","set","remove","clear"}.Contains(x)); When(x => x.Operation == "add" || x.Operation == "set", () => { RuleFor(x => x.ProductId).GreaterThan(0); RuleFor(x => x.Quantity).InclusiveBetween(1,999); }); }
}
