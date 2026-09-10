using FluentValidation;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
public sealed class GetAccessProfilesQueryValidator : AbstractValidator<GetAccessProfilesQuery>
{
    public GetAccessProfilesQueryValidator()
    {
        RuleFor(x => x.ActorId).GreaterThan(0);
    }
}
