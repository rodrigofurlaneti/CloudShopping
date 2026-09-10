using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackerById;
public sealed class GetLogTrackerByIdQueryHandler(ILogTrackerRepository repository) : IRequestHandler<GetLogTrackerByIdQuery, Result<LogTrackerView?>>
{
    public Task<Result<LogTrackerView?>> Handle(GetLogTrackerByIdQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<LogTrackerView?> ExecuteAsync(GetLogTrackerByIdQuery request, CancellationToken ct)
    {
        var entry = await repository.GetByIdAsync(request.Id, ct);
        return entry == null ? null : LogTrackerView.From(entry);
    }
}
