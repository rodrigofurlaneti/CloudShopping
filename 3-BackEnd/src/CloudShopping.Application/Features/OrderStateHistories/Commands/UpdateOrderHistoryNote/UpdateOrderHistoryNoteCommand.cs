using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.OrderStateHistories.Commands.UpdateOrderHistoryNote
{
    public sealed record UpdateOrderHistoryNoteCommand(int HistoryId, string NewNote) : IRequest<Result>;
}
