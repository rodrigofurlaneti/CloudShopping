using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.UpdatePaymentDecline
{
    public sealed record UpdatePaymentDeclinedCommand(int OrderId, int PaymentId) : IRequest<Result>;
}
