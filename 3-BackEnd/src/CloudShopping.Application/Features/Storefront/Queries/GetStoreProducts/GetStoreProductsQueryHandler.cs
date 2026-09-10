using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProducts;
public sealed class GetStoreProductsQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreProductsQuery, StorePage<StoreProductSummary>>
{
    public async Task<StorePage<StoreProductSummary>> Handle(GetStoreProductsQuery request, CancellationToken ct)
    {
        return await repository.Products(request.Search, request.DepartmentId, request.Page, request.PageSize, ct);
    }
}
