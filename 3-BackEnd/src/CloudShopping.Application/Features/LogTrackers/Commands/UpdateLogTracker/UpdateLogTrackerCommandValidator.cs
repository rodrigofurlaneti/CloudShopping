using FluentValidation;
using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Features.LogTrackers.Commands.UpdateLogTracker;
public sealed class UpdateLogTrackerCommandValidator : AbstractValidator<UpdateLogTrackerCommand>
{
    public UpdateLogTrackerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data).Custom((data, context) =>
        {
            try { LogTracker.Validate(data); }
            catch (ArgumentException e) { context.AddFailure(e.Message); }
        });
    }
}
