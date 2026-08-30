using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.DeclinePayment
{
    public sealed record DeclinePaymentCommand(int OrderId, int PaymentId) : IRequest<Result>;
}
