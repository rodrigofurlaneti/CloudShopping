using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed
{
    public sealed class MarkDeliveryFailedCommandValidator : AbstractValidator<MarkDeliveryFailedCommand>
    {
        public MarkDeliveryFailedCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O ID do lojista (Tenant) é inválido.");
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("O motivo da falha na entrega é obrigatório.")
                .MaximumLength(500)
                .WithMessage("O motivo deve ter no máximo 500 caracteres.");
        }
    }
}
