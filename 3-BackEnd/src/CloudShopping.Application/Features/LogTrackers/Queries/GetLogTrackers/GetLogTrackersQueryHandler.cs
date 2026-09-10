using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackers;
public sealed class GetLogTrackersQueryHandler(ILogTrackerRepository repository) : IRequestHandler<GetLogTrackersQuery, Result<LogTrackerPage>>
{
    public Task<Result<LogTrackerPage>> Handle(GetLogTrackersQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<LogTrackerPage> ExecuteAsync(GetLogTrackersQuery request, CancellationToken ct)
    {
        var (items, count) = await repository.GetPaginatedAsync(request.Page, request.PageSize, request.Outcome, ct);
        return new(items.Select(LogTrackerView.From).ToArray(), count, request.Page, request.PageSize);
    }
}
