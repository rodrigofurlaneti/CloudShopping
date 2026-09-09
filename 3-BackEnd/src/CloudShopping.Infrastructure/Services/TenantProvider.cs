using CloudShopping.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
namespace CloudShopping.Infrastructure.Services;
public sealed class TenantProvider(IHttpContextAccessor accessor) : ITenantProvider
{
    public int GetTenantId() => accessor.HttpContext?.Items["TenantId"] is int tenant ? tenant : 0;
}
