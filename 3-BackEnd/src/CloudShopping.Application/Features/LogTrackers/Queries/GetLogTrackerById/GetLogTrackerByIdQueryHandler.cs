using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackerById;
public sealed class GetLogTrackerByIdQueryHandler(ILogTrackerRepository repository) : IRequestHandler<GetLogTrackerByIdQuery, LogTrackerView?>
{
    public async Task<LogTrackerView?> Handle(GetLogTrackerByIdQuery request, CancellationToken ct)
    {
        var entry = await repository.GetByIdAsync(request.Id, ct);
        return entry == null ? null : LogTrackerView.From(entry);
    }
}
