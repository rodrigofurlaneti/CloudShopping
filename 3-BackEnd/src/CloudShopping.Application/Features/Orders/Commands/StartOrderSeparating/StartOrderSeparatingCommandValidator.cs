using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.StartOrderSeparating
{
    public sealed class StartOrderSeparatingCommandValidator : AbstractValidator<StartOrderSeparatingCommand>
    {
        public StartOrderSeparatingCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O ID do lojista (Tenant) é inválido.");
        }
    }
}
