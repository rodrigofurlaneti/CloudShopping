using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Queries.GetCoupons;
public sealed class GetCouponsQueryHandler(ICouponRepository repository) : IRequestHandler<GetCouponsQuery, Result<IReadOnlyList<CouponView>>>
{
    public Task<Result<IReadOnlyList<CouponView>>> Handle(GetCouponsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<CouponView>> ExecuteAsync(GetCouponsQuery request, CancellationToken ct) =>
        (await repository.GetPageAsync(request.Page, 20, ct)).Select(CouponView.From).ToArray();
}
