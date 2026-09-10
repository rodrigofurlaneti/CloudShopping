using FluentValidation;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackers;
public sealed class GetLogTrackersQueryValidator : AbstractValidator<GetLogTrackersQuery>
{
    public GetLogTrackersQueryValidator()
    {
        RuleFor(x=>x.Page).InclusiveBetween(1,100000);
        RuleFor(x=>x.PageSize).InclusiveBetween(1,200);
        RuleFor(x=>x.Outcome).Must(x=>x is null or "Success" or "Error" or "Cancelled");
    }
}
