using FluentValidation;
namespace CloudShopping.Application.Features.Sessions.Queries.GetSessionPermissions;
public sealed class GetSessionPermissionsQueryValidator : AbstractValidator<GetSessionPermissionsQuery>
{
    public GetSessionPermissionsQueryValidator() { RuleFor(x=>x.Caller).NotNull(); }
}
