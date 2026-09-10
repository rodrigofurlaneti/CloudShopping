using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Features.LogTrackers.Contracts;
public sealed record LogTrackerView(long Id, int? TenantId, LogTrackerData Data, bool IsSuccess, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt)
{
    public static LogTrackerView From(LogTracker entry) => new(entry.Id, entry.TenantId,
        new(entry.AppUserId, entry.ActorType, entry.TraceId, entry.DirectoryName, entry.ClassName, entry.MethodName, entry.Outcome, entry.RequestAborted, entry.ExecutionTimeMs, entry.HttpMethod, entry.HttpStatusCode, entry.ExceptionType, entry.Message, entry.ErrorMessage, entry.StackTrace, entry.IpAddress), entry.IsSuccess, entry.IsActive, entry.CreatedAt, entry.UpdatedAt);
}
public sealed record LogTrackerPage(IReadOnlyList<LogTrackerView> Items, int TotalCount, int Page, int PageSize);
