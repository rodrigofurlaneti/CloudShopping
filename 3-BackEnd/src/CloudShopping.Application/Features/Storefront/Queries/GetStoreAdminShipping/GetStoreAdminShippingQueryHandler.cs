using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreAdminShipping;
public sealed class GetStoreAdminShippingQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreAdminShippingQuery, IReadOnlyList<StoreShippingView>>
{
    public async Task<IReadOnlyList<StoreShippingView>> Handle(GetStoreAdminShippingQuery request, CancellationToken ct)
    {
        return await repository.Shipping(null, ct);
    }
}
