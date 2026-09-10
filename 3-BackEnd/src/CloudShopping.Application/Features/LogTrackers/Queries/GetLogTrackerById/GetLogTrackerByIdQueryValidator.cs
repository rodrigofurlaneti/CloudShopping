using FluentValidation;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackerById;
public sealed class GetLogTrackerByIdQueryValidator : AbstractValidator<GetLogTrackerByIdQuery>
{
    public GetLogTrackerByIdQueryValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
    }
}
