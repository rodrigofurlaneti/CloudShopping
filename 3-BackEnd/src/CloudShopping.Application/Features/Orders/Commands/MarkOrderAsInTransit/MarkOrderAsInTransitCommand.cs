using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInTransit
{
    public sealed record MarkOrderAsInTransitCommand(int OrderId) : IRequest<Result>;
}
