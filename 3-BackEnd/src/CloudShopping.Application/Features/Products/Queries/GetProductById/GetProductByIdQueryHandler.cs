using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductViewModel?>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product is null) return null;

            return new ProductViewModel(
                product.Id,
                product.DepartmentId,
                product.Sku,
                product.Name,
                product.Price,
                product.PhysicalStock,
                product.ReservedStock,
                product.AvailableStock,
                product.Location?.Aisle,
                product.Location?.Rack,
                product.Location?.Level,
                product.Location?.Position,
                product.Images.Select(i => new ProductImageViewModel(i.Id, i.FileName, i.FilePath, i.IsPrimary, i.DisplayOrder)).ToList());
        }
    }
}
