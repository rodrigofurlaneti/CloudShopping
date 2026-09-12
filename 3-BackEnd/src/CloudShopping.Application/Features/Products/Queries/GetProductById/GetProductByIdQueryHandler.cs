using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.Caching;
using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductViewModel?>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICacheService _cache;

        public GetProductByIdQueryHandler(IProductRepository productRepository, ITenantProvider tenantProvider, ICacheService cache)
        {
            _productRepository = productRepository;
            _tenantProvider = tenantProvider;
            _cache = cache;
        }

        public Task<Result<ProductViewModel?>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);

        // Cache Longo (produtos - dados fixos, tarefa de cache Redis): os campos estáticos
        // (nome, preço, endereçamento, imagens) vêm do cache quando disponíveis; o estoque
        // (PhysicalStock/ReservedStock/AvailableStock) é sempre lido ao vivo do MySQL
        // (GetStockAsync) e mesclado antes de devolver o ViewModel — nunca é cacheado.
        private async Task<ProductViewModel?> ExecuteAsync(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cacheKey = CacheKeys.TenantEntity(tenantId, CacheKeys.Products, request.Id);

            var stable = await _cache.GetOrCreateAsync(cacheKey, CacheTtl.Long, async ct =>
            {
                var product = await _productRepository.GetByIdAsync(request.Id, ct);
                return product is null ? null : ProductStaticView.From(product);
            }, cancellationToken);

            if (stable is null) return null;

            var stock = await _productRepository.GetStockAsync(request.Id, cancellationToken);
            if (stock is null)
            {
                // O produto existia quando o cache estático foi gravado mas não existe
                // mais (excluído/desativado) — descarta a entrada cacheada.
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return null;
            }

            return stable.ToViewModel(stock.Value.PhysicalStock, stock.Value.ReservedStock);
        }
    }
}
