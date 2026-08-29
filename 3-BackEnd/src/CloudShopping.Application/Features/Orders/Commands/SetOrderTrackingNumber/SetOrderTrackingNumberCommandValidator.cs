using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.SetOrderTrackingNumber
{
    public sealed class SetOrderTrackingNumberCommandValidator : AbstractValidator<SetOrderTrackingNumberCommand>
    {
        public SetOrderTrackingNumberCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O ID do lojista (Tenant) é inválido.");
            RuleFor(x => x.TrackingNumber)
                .NotEmpty()
                .WithMessage("O código de rastreio é obrigatório.")
                .MaximumLength(100)
                .WithMessage("O código de rastreio deve ter no máximo 100 caracteres.");
        }
    }
}
