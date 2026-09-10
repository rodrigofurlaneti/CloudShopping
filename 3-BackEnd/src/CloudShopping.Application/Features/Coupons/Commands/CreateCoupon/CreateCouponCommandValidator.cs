using FluentValidation;
namespace CloudShopping.Application.Features.Coupons.Commands.CreateCoupon;
public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Kind).NotEmpty();
    }
}
