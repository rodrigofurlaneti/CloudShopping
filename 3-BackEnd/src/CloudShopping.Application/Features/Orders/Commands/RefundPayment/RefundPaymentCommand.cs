using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.RefundPayment
{
    public sealed record RefundPaymentCommand(int OrderId, int PaymentId) : IRequest<Result>;
}
