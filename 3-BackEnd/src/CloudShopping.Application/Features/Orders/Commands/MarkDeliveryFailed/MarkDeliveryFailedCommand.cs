using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed
{
    public sealed record MarkDeliveryFailedCommand(
        int OrderId,
        string Reason) : IRequest<Result>;
}