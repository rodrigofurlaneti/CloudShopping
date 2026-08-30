using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.ApprovePayment
{
    public sealed record ApprovePaymentCommand(
        int OrderId,
        int PaymentId) : IRequest<Result>;
}