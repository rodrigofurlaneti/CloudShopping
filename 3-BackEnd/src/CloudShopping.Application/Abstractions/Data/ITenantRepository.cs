using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Tenants;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface ITenantRepository : IRepository<Tenant, int> 
    {
        Task<(IEnumerable<Tenant> Items, int TotalCount)> GetPaginatedAsync(int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);

    }
}