using Microsoft.AspNetCore.Http;

namespace CloudShopping.Api.Security;

public sealed class RequestCancellationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.RequestAborted.IsCancellationRequested)
        {
            FinishCancelledRequest(context);
            return;
        }

        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            FinishCancelledRequest(context);
        }
    }

    private static void FinishCancelledRequest(HttpContext context)
    {
        // The client may already be disconnected. Do not write a response body or change cookies.
        if (!context.Response.HasStarted)
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
    }
}
