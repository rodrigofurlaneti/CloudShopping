using FluentValidation;
namespace CloudShopping.Application.Features.OrderStateHistories.Commands.DeactivateOrderHistory
{
    public sealed class DeactivateOrderHistoryCommandValidator : AbstractValidator<DeactivateOrderHistoryCommand>
    {
        public DeactivateOrderHistoryCommandValidator()
        {
            RuleFor(x => x.HistoryId).GreaterThan(0);
        }
    }
}
