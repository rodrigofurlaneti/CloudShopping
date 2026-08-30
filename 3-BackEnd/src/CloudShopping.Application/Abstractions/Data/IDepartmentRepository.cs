using CloudShopping.Domain.Entities.Products;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface IDepartmentRepository : IRepository<Department, int>
    {
        Task<IEnumerable<Department>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<bool> SlugExistsAsync(int tenantId, string slug, CancellationToken cancellationToken = default);
    }
}