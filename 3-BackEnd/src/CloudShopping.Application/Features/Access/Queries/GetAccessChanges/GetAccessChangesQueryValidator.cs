using FluentValidation;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessChanges;
public sealed class GetAccessChangesQueryValidator : AbstractValidator<GetAccessChangesQuery>
{
    public GetAccessChangesQueryValidator()
    {
        RuleFor(x => x.ActorId).GreaterThan(0);
        RuleFor(x => x.Page).InclusiveBetween(1, int.MaxValue / 20);
    }
}
