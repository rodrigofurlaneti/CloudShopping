using FluentValidation;
namespace CloudShopping.Application.Features.Access.Queries.GetUserPermissions;
public sealed class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
    public GetUserPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
