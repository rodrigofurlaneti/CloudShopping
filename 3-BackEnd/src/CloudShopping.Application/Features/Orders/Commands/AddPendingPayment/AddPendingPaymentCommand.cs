using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.AddPendingPayment
{
    public sealed record AddPendingPaymentCommand(
        int OrderId,
        string PaymentMethod,
        decimal Amount) : IRequest<Result>;
}