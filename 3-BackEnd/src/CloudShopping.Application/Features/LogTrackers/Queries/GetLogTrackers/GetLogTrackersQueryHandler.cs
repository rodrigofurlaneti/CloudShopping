using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackers;
public sealed class GetLogTrackersQueryHandler(ILogTrackerRepository repository) : IRequestHandler<GetLogTrackersQuery, LogTrackerPage>
{
    public async Task<LogTrackerPage> Handle(GetLogTrackersQuery request, CancellationToken ct)
    {
        var (items, count) = await repository.GetPaginatedAsync(request.Page, request.PageSize, request.Outcome, ct);
        return new(items.Select(LogTrackerView.From).ToArray(), count, request.Page, request.PageSize);
    }
}
