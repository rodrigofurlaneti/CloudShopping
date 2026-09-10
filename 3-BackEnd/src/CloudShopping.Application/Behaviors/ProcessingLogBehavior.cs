using System.Diagnostics;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers;
using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Behaviors;

public sealed class ProcessingLogBehavior<TRequest, TResponse>(
    ProcessingLogRecorder recorder, IProcessingLogContext? context = null)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        Exception? failure = null;
        var failedResult = false;
        try
        {
            var response = await next(cancellationToken);
            failedResult = response is Result { IsFailure: true };
            return response;
        }
        catch (Exception ex) { failure = ex; throw; }
        finally
        {
            var aborted = context?.RequestAborted == true;
            var outcome = failure is OperationCanceledException || aborted ? "Cancelled"
                : failure != null || failedResult ? "Error" : "Success";
            // Never serialize requests, results or exception messages: they may contain credentials.
            var frames = failure == null ? null : string.Join("\n", new StackTrace(failure, false).GetFrames()
                .Take(30).Select(f => f.GetMethod()).Where(m => m != null)
                .Select(m => $"{m!.DeclaringType?.FullName}.{m.Name}"));
            var data = new LogTrackerData(context?.ActorId, context?.ActorType,
                context?.TraceId ?? Activity.Current?.TraceId.ToString(), "Application",
                typeof(TRequest).Name, "Handle", outcome, aborted,
                (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds, null, null,
                failure?.GetType().FullName,
                failedResult ? "Caso de uso retornou Result de falha." : outcome == "Success"
                    ? "Caso de uso concluído." : outcome == "Cancelled" ? "Caso de uso cancelado." : "Caso de uso lançou exceção.",
                failure == null ? null : "Consulte o tipo da exceção e o TraceId.", frames, null);
            await recorder.RecordAsync(context?.TenantId, data);
        }
    }
}

