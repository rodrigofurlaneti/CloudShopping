using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreAddresses;
public sealed class GetStoreAddressesQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreAddressesQuery, IReadOnlyList<StoreAddressView>>
{
    public async Task<IReadOnlyList<StoreAddressView>> Handle(GetStoreAddressesQuery request, CancellationToken ct)
    {
        return await repository.Addresses(request.CustomerId, ct);
    }
}
