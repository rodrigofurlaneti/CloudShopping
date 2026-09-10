using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Commands.CreateCoupon;
public sealed class CreateCouponCommandHandler(ICouponRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenant) : IRequestHandler<CreateCouponCommand, Result<CouponView>>
{
    public Task<Result<CouponView>> Handle(CreateCouponCommand request, CancellationToken ct) => CommandExecution.Run(async () =>
    {
        var coupon = Coupon.Create(tenant.GetTenantId(), request.Code, request.Kind, request.Value,
            request.MinimumSubtotal, request.UsageLimit, request.PerCustomerLimit, request.StartsAt, request.EndsAt);
        if (await repository.CodeExistsAsync(coupon.Code, ct)) throw new InvalidOperationException("Código já cadastrado.");
        await repository.AddAsync(coupon, ct);
        await unitOfWork.CommitAsync(ct);
        return CouponView.From(coupon);
    });
}
