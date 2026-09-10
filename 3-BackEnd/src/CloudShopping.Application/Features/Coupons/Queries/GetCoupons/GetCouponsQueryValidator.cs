using FluentValidation;
namespace CloudShopping.Application.Features.Coupons.Queries.GetCoupons;
public sealed class GetCouponsQueryValidator : AbstractValidator<GetCouponsQuery>
{
    public GetCouponsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, int.MaxValue / 20);
    }
}
