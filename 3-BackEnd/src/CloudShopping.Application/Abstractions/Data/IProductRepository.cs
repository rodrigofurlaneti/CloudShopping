using CloudShopping.Domain.Entities.Products;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProductRepository : IRepository<Product, int>
    {
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    }
}
