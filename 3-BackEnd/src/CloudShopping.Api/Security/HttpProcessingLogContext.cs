using System.Security.Claims;
using CloudShopping.Application.Abstractions.Data;

namespace CloudShopping.Api.Security;

public sealed class HttpProcessingLogContext(IHttpContextAccessor accessor) : IProcessingLogContext
{
    public int? TenantId => accessor.HttpContext?.Items["TenantId"] is int id && id > 0 ? id : null;
    public string? ActorType => accessor.HttpContext?.User.IsInRole("Administrator") == true ? "Administrator"
        : accessor.HttpContext?.User.IsInRole("Customer") == true ? "Customer" : null;
    public long? ActorId => ActorType != null && long.TryParse(
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;
    public string? TraceId => accessor.HttpContext?.TraceIdentifier;
    public bool RequestAborted => accessor.HttpContext?.RequestAborted.IsCancellationRequested == true;
}
