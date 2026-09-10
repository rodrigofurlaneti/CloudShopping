namespace CloudShopping.Domain.Entities.Diagnostics;

public sealed record LogTrackerData(
    long? AppUserId,
    string? ActorType,
    string? TraceId,
    string? DirectoryName,
    string ClassName,
    string MethodName,
    string Outcome,
    bool RequestAborted,
    long? ExecutionTimeMs,
    string? HttpMethod,
    int? HttpStatusCode,
    string? ExceptionType,
    string? Message,
    string? ErrorMessage,
    string? StackTrace,
    string? IpAddress);
