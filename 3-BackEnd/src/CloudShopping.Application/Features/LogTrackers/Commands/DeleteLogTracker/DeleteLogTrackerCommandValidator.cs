using FluentValidation;
using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Features.LogTrackers.Commands.DeleteLogTracker;
public sealed class DeleteLogTrackerCommandValidator : AbstractValidator<DeleteLogTrackerCommand>
{
    public DeleteLogTrackerCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
