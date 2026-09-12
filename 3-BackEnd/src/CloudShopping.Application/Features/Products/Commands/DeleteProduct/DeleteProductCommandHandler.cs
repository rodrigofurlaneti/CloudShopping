using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.DeleteProduct
{
    public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;

        public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, ICacheService cache)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null)
                return Result.Failure(new Error("Product.NotFound", "Produto não encontrado."));

            // Soft delete: mantém histórico/integridade referencial com pedidos já realizados.
            product.Deactivate();
            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Invalida o cache Redis: um produto desativado não deve continuar sendo
            // servido pela ficha cacheada (nem por Id, nem por SKU).
            await _cache.RemoveManyAsync(new[]
            {
                CacheKeys.TenantEntity(product.TenantId, CacheKeys.Products, product.Id),
                CacheKeys.ProductBySku(product.TenantId, product.Sku)
            }, cancellationToken);

            return Result.Success();
        }
    }
}
