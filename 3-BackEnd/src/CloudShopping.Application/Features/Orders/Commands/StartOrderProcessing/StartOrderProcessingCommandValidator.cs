using FluentValidation;
namespace CloudShopping.Application.Features.Orders.Commands.StartOrderProcessing
{
    public sealed class StartOrderProcessingCommandValidator : AbstractValidator<StartOrderProcessingCommand>
    {
        public StartOrderProcessingCommandValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0);
            RuleFor(x => x.TenantId).GreaterThan(0);
        }
    }
}
