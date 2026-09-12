using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public ProductRepository(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            return await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var idList = ids.ToList();

            return await _context.Products
                .Where(p => idList.Contains(p.Id) && p.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public Task<bool> IsSlugOrVariantInUseAsync(int exceptProductId, string slug, string? familyCode, string? variantLabel, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            return _context.Products.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.Id != exceptProductId &&
                (x.Slug == slug || familyCode != null && x.FamilyCode == familyCode && x.VariantLabel == variantLabel), cancellationToken);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            return await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Sku == sku && p.TenantId == tenantId, cancellationToken);
        }

        // Projeção enxuta (2 colunas, sem tracking, sem Include de imagens) para a
        // tarefa de cache Redis: o estoque é sempre lido ao vivo, nunca cacheado, então
        // este método é chamado em toda leitura de ficha de produto (id ou SKU), esteja
        // o restante do produto vindo do cache ou não.
        public async Task<(int PhysicalStock, int ReservedStock)?> GetStockAsync(int id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var row = await _context.Products.AsNoTracking()
                .Where(p => p.Id == id && p.TenantId == tenantId)
                .Select(p => new { p.PhysicalStock, p.ReservedStock })
                .FirstOrDefaultAsync(cancellationToken);

            return row is null ? null : (row.PhysicalStock, row.ReservedStock);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPaginatedAsync(
            int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Products.Where(p => p.TenantId == tenantId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Include(Images) é necessário aqui: o ProductSummaryViewModel usa
            // p.Images para resolver a foto principal da listagem — sem o Include,
            // a coleção sempre viria vazia e a listagem mostraria "sem imagem" para
            // todo produto, mesmo com fotos já enviadas.
            //
            // Esta listagem paginada NÃO passa pelo cache Redis: cada página/termo de
            // busca gera uma combinação diferente de chave e já inclui o estoque
            // (PhysicalStock/ReservedStock/AvailableStock) por item, que a tarefa de
            // cache proíbe cachear — cachear só os campos fixos aqui exigiria uma
            // segunda consulta de estoque por item da página, o que anula o ganho.
            // A ficha de produto (GetProductById/GetProductBySku, o caminho mais
            // repetido) é quem usa o cache — ver GetProductByIdQueryHandler.
            var items = await query
                .Include(p => p.Images)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
