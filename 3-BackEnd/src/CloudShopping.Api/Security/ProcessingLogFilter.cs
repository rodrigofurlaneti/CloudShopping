using System.Diagnostics;
using System.Security.Claims;
using CloudShopping.Application.Features.LogTrackers;
using CloudShopping.Domain.Entities.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
namespace CloudShopping.Api.Security;

public sealed class ProcessingLogFilter(ProcessingLogRecorder recorder) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var started = Stopwatch.GetTimestamp();
        Exception? failure = null;
        try { var executed = await next(); if (!executed.ExceptionHandled) failure = executed.Exception; }
        catch (Exception ex) { failure = ex; throw; }
        finally
        {
            var http = context.HttpContext;
            var cancelled = failure is OperationCanceledException || http.RequestAborted.IsCancellationRequested;
            var status = failure != null ? (cancelled && http.RequestAborted.IsCancellationRequested ? 499 : 500) : http.Response.StatusCode;
            var outcome = cancelled ? "Cancelled" : status >= 400 ? "Error" : "Success";
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            int? tenant = http.Items["TenantId"] is int id && id > 0 ? id : null;
            var role = http.User.IsInRole("Administrator") ? "Administrator" : http.User.IsInRole("Customer") ? "Customer" : null;
            long? actor = role != null && long.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var subject) && subject > 0 ? subject : null;
            // No request body, response body, query string, exception message, IP or source file paths.
            var frames = failure == null ? null : string.Join("\n", (new StackTrace(failure, false).GetFrames() ?? [])
                .Take(30).Select(f => f.GetMethod()).Where(m => m != null).Select(m => $"{m!.DeclaringType?.FullName}.{m.Name}"));
            var data = new LogTrackerData(actor, role, http.TraceIdentifier, "Controllers",
                descriptor?.ControllerTypeInfo.Name ?? "UnknownController", descriptor?.ActionName ?? "UnknownAction",
                outcome, http.RequestAborted.IsCancellationRequested, (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                http.Request.Method, status, failure?.GetType().FullName,
                outcome == "Success" ? "Processamento concluído." : outcome == "Cancelled" ? "Processamento cancelado." : "Processamento retornou falha.",
                failure == null ? null : "Consulte o tipo da exceção e o TraceId.", frames, null);
            await recorder.RecordAsync(tenant, data);
        }
    }
}
