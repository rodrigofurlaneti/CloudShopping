using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsDelivered
{
    public sealed record MarkOrderAsDeliveredCommand(int OrderId) : IRequest<Result>;
}