using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Products;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProductRepository : IRepository<Product, int>
    {
        Task<(IEnumerable<Cart> Items, int TotalCount)> GetPaginatedAsync(int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    }
}
