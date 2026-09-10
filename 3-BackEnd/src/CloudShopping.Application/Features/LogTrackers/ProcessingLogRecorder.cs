using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Diagnostics;
using Microsoft.Extensions.Logging;
namespace CloudShopping.Application.Features.LogTrackers;

public sealed class ProcessingLogRecorder(IProcessingLogWriter writer, ILogger<ProcessingLogRecorder> logger)
{
    public async Task RecordAsync(int? tenantId, LogTrackerData data)
    {
        // A disconnected HTTP request must not cancel its own diagnostic record.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try { await writer.WriteAsync(LogTracker.Create(tenantId, data), timeout.Token); }
        catch (Exception ex)
        {
            // Do not replace the original response or leak connection details via exception.Message.
            logger.LogWarning("Diagnostic persistence failed. TraceId={TraceId}; FailureType={FailureType}", data.TraceId, ex.GetType().FullName);
        }
    }
}
