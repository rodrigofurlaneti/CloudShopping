using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Queries.GetCoupons;
public sealed class GetCouponsQueryHandler(ICouponRepository repository) : IRequestHandler<GetCouponsQuery, IReadOnlyList<CouponView>>
{
    public async Task<IReadOnlyList<CouponView>> Handle(GetCouponsQuery request, CancellationToken ct) =>
        (await repository.GetPageAsync(request.Page, 20, ct)).Select(CouponView.From).ToArray();
}
