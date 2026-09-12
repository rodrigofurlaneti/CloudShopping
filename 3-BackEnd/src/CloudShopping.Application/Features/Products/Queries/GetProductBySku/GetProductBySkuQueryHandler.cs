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

namespace CloudShopping.Application.Features.Products.Queries.GetProductBySku
{
    public sealed class GetProductBySkuQueryHandler : IRequestHandler<GetProductBySkuQuery, Result<ProductViewModel?>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICacheService _cache;

        public GetProductBySkuQueryHandler(IProductRepository productRepository, ITenantProvider tenantProvider, ICacheService cache)
        {
            _productRepository = productRepository;
            _tenantProvider = tenantProvider;
            _cache = cache;
        }

        public Task<Result<ProductViewModel?>> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken) => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);

        // Mesma estratégia de GetProductByIdQueryHandler (cache dos campos fixos, estoque
        // sempre ao vivo), com chave própria por SKU — ver CacheKeys.ProductBySku.
        private async Task<ProductViewModel?> ExecuteAsync(GetProductBySkuQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cacheKey = CacheKeys.ProductBySku(tenantId, request.Sku);

            var stable = await _cache.GetOrCreateAsync(cacheKey, CacheTtl.Long, async ct =>
            {
                var product = await _productRepository.GetBySkuAsync(request.Sku, ct);
                return product is null ? null : ProductStaticView.From(product);
            }, cancellationToken);

            if (stable is null) return null;

            var stock = await _productRepository.GetStockAsync(stable.Id, cancellationToken);
            if (stock is null)
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return null;
            }

            return stable.ToViewModel(stock.Value.PhysicalStock, stock.Value.ReservedStock);
        }
    }
}
