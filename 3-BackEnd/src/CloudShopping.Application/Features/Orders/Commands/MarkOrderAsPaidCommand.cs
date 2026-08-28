using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed record MarkOrderAsPaidCommand(
        int OrderId,
        string PaymentMethod,
        decimal Amount) : IRequest<Result>;
}
