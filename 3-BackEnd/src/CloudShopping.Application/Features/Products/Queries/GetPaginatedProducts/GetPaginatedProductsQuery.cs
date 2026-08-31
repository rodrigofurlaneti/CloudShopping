using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Products.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Products.Queries.GetPaginatedProducts
{
    // Não existia nenhuma consulta de listagem para Products (só GetById/GetBySku).
    // IProductRepository.GetPaginatedAsync já existia mas não estava exposta por
    // nenhuma Query — mesmo padrão de gap-fill usado em Customers/OrderSectors/OrderStatus.
    public sealed record GetPaginatedProductsQuery(
        int Page = 1,
        int PageSize = 10,
        string? SearchTerm = null) : IRequest<Result<PagedResult<ProductSummaryViewModel>>>;
}
