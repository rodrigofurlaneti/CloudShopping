using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreShipping;
public sealed class GetStoreShippingQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreShippingQuery, IReadOnlyList<StoreShippingView>>
{
    public async Task<IReadOnlyList<StoreShippingView>> Handle(GetStoreShippingQuery request, CancellationToken ct)
    {
        var address = await repository.Address(request.CustomerId, request.AddressId, ct) ?? throw new KeyNotFoundException();
        return await repository.Shipping(address.ZipCode, ct);
    }
}
