using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Queries.GetProductBySku
{
    public sealed class GetProductBySkuQueryHandler : IRequestHandler<GetProductBySkuQuery, ProductViewModel?>
    {
        private readonly IProductRepository _productRepository;

        public GetProductBySkuQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductViewModel?> Handle(GetProductBySkuQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
            if (product is null) return null;

            return new ProductViewModel(
                product.Id,
                product.Sku,
                product.Name,
                product.Price,
                product.PhysicalStock,
                product.ReservedStock,
                product.AvailableStock,
                product.Images.Select(i => new ProductImageViewModel(i.Id, i.FileName, i.FilePath, i.IsPrimary, i.DisplayOrder)).ToList());
        }
    }
}
