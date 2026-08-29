using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.SetOrderTrackingNumber
{
    public sealed record SetOrderTrackingNumberCommand(int OrderId, int TenantId, string TrackingNumber) : IRequest<Result>;
}
