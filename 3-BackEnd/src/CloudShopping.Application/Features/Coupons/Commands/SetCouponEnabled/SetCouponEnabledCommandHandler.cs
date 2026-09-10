using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Coupons.Commands.SetCouponEnabled;
public sealed class SetCouponEnabledCommandHandler(ICouponRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<SetCouponEnabledCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(SetCouponEnabledCommand request, CancellationToken ct) => CommandExecution.Run(async () =>
    {
        var coupon = await repository.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("Cupom não encontrado.");
        if (coupon.Enabled == request.Enabled) return Unit.Value;
        if (coupon.Version != request.Version) throw new InvalidOperationException("Cupom alterado; recarregue.");
        coupon.SetEnabled(request.Enabled);
        await unitOfWork.CommitAsync(ct);
        return Unit.Value;
    });
}
