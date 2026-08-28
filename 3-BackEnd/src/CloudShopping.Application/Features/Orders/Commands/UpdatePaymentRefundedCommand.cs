using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed record UpdatePaymentRefundedCommand(int OrderId, int PaymentId) : IRequest<Result>;
}
