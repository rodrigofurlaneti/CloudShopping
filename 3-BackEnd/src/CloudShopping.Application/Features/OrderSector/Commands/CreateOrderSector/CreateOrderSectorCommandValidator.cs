using FluentValidation;
namespace CloudShopping.Application.Features.OrderSector.Commands.CreateOrderSector;
public sealed class CreateOrderSectorCommandValidator : AbstractValidator<CreateOrderSectorCommand>
{
    public CreateOrderSectorCommandValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);
    }
}
