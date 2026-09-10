namespace CloudShopping.Application.Abstractions.Data;

public interface IProcessingLogContext
{
    int? TenantId { get; }
    long? ActorId { get; }
    string? ActorType { get; }
    string? TraceId { get; }
    bool RequestAborted { get; }
}
