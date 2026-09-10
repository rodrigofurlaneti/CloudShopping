using FluentValidation;
namespace CloudShopping.Application.Features.Coupons.Commands.SetCouponEnabled;
public sealed class SetCouponEnabledCommandValidator : AbstractValidator<SetCouponEnabledCommand>
{
    public SetCouponEnabledCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().Length(32);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
