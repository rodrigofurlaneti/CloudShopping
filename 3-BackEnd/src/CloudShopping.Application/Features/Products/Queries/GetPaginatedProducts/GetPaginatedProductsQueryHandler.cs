using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Queries.GetPaginatedProducts
{
    public sealed class GetPaginatedProductsQueryHandler
        : IRequestHandler<GetPaginatedProductsQuery, Result<PagedResult<ProductSummaryViewModel>>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantProvider _tenantProvider;

        public GetPaginatedProductsQueryHandler(
            IProductRepository productRepository,
            ITenantProvider tenantProvider)
        {
            _productRepository = productRepository;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<PagedResult<ProductSummaryViewModel>>> Handle(
            GetPaginatedProductsQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var (items, totalCount) = await _productRepository.GetPaginatedAsync(
                tenantId,
                request.Page,
                request.PageSize,
                request.SearchTerm,
                cancellationToken);

            var responseItems = items.Select(p => new ProductSummaryViewModel(
                p.Id,
                p.DepartmentId,
                p.Sku,
                p.Name,
                p.Price,
                p.PhysicalStock,
                p.ReservedStock,
                p.AvailableStock,
                p.Location != null,
                (p.Images.FirstOrDefault(i => i.IsPrimary) ?? p.Images.FirstOrDefault())?.FilePath
            )).ToList().AsReadOnly();

            var pagedResult = new PagedResult<ProductSummaryViewModel>(
                responseItems,
                totalCount,
                request.Page,
                request.PageSize);

            return Result.Success(pagedResult);
        }
    }
}
