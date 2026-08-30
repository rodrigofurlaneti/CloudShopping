using CloudShopping.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace CloudShopping.Infrastructure.Services
{
    /// <summary>
    /// Resolve o Tenant corrente a partir do cabeçalho HTTP "X-Tenant-Id" da requisição.
    /// Se o cabeçalho não estiver presente ou for inválido, assume o Tenant padrão (1),
    /// permitindo o uso da API/Swagger em ambiente de desenvolvimento sem cabeçalho customizado.
    /// </summary>
    public sealed class TenantProvider : ITenantProvider
    {
        private const int DefaultTenantId = 1;
        private readonly int _tenantId;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is not null &&
                httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
                int.TryParse(tenantHeader, out var parsedTenantId) &&
                parsedTenantId > 0)
            {
                _tenantId = parsedTenantId;
            }
            else
            {
                _tenantId = DefaultTenantId;
            }
        }

        public int GetTenantId() => _tenantId;
    }
}
