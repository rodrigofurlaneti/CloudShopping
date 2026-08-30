using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.UpdatePaymentApproved
{
    public sealed record UpdatePaymentApprovedCommand(
        int OrderId,
        int PaymentId) : IRequest<Result>;
}