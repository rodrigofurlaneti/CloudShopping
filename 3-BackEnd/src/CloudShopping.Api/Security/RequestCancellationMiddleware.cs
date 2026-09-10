using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CloudShopping.Api.Security;

public sealed class RequestCancellationMiddleware(RequestDelegate next, ILogger<RequestCancellationMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        if (context.RequestAborted.IsCancellationRequested)
        {
            LogCancellation(context, started, true);
            FinishCancelledRequest(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            LogCancellation(context, started, true);
            FinishCancelledRequest(context);
        }
        catch (OperationCanceledException)
        {
            LogCancellation(context, started, false);
            throw;
        }
    }

    private void LogCancellation(HttpContext context, long started, bool requestAborted)
    {
        // Exclude query strings, bodies, credentials and exception messages.
        logger?.Log(requestAborted ? LogLevel.Information : LogLevel.Warning,
            new EventId(requestAborted ? 499 : 500, "RequestCancellation"),
            "Cancellation: RequestAborted={RequestAborted}; Method={Method}; Path={Path}; ElapsedMs={ElapsedMs}; TraceId={TraceId}",
            requestAborted, context.Request.Method, context.Request.Path.Value,
            Stopwatch.GetElapsedTime(started).TotalMilliseconds, context.TraceIdentifier);
    }

    private static void FinishCancelledRequest(HttpContext context)
    {
        // The client may already be disconnected. Do not write a response body or change cookies.
        if (!context.Response.HasStarted)
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
    }
}
