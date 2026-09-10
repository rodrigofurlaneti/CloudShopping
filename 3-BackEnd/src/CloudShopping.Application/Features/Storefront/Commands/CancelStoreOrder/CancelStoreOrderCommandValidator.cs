using FluentValidation;
namespace CloudShopping.Application.Features.Storefront.Commands.CancelStoreOrder;
public sealed class CancelStoreOrderCommandValidator : AbstractValidator<CancelStoreOrderCommand>
{
    public CancelStoreOrderCommandValidator() { RuleFor(x => x.CustomerId).GreaterThan(0); RuleFor(x => x.Id).GreaterThan(0); }
}
