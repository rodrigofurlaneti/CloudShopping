using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsDelivered
{
    public sealed class MarkOrderAsDeliveredCommandValidator : AbstractValidator<MarkOrderAsDeliveredCommand>
    {
        public MarkOrderAsDeliveredCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
        }
    }
}