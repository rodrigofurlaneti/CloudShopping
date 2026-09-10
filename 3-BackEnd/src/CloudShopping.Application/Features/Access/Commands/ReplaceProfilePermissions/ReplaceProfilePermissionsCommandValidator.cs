using FluentValidation;
namespace CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
public sealed class ReplaceProfilePermissionsCommandValidator : AbstractValidator<ReplaceProfilePermissionsCommand>
{
    public ReplaceProfilePermissionsCommandValidator()
    {
        RuleFor(x => x.ActorId).GreaterThan(0);
        RuleFor(x => x.ProfileId).GreaterThan(0);
        RuleFor(x => x.Expected).NotNull();
        RuleFor(x => x.Permissions).NotNull();
    }
}
