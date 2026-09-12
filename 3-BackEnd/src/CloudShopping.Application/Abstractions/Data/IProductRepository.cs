using CloudShopping.Domain.Entities.Products;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProductRepository : IRepository<Product, int>
    {
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPaginatedAsync(int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<bool> IsSlugOrVariantInUseAsync(int exceptProductId, string slug, string? familyCode, string? variantLabel, CancellationToken cancellationToken = default);

        // Leitura enxuta (só PhysicalStock/ReservedStock, sem carregar o restante do
        // agregado nem as imagens) usada pela tarefa de cache Redis: o estoque nunca é
        // cacheado, então toda leitura de ficha de produto busca este valor ao vivo no
        // MySQL, mesmo quando os demais campos vieram do cache.
        Task<(int PhysicalStock, int ReservedStock)?> GetStockAsync(int id, CancellationToken cancellationToken = default);
    }
}
