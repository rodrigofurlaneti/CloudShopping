using FluentValidation;
using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Features.LogTrackers.Commands.CreateLogTracker;
public sealed class CreateLogTrackerCommandValidator : AbstractValidator<CreateLogTrackerCommand>
{
    public CreateLogTrackerCommandValidator()
    {
        
        RuleFor(x => x.Data).Custom((data, context) =>
        {
            try { LogTracker.Validate(data); }
            catch (ArgumentException e) { context.AddFailure(e.Message); }
        });
    }
}
