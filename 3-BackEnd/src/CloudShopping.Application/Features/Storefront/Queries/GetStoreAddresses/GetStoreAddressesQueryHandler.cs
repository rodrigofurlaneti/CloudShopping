using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreAddresses;
public sealed class GetStoreAddressesQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreAddressesQuery, Result<IReadOnlyList<StoreAddressView>>>
{
    public Task<Result<IReadOnlyList<StoreAddressView>>> Handle(GetStoreAddressesQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<StoreAddressView>> ExecuteAsync(GetStoreAddressesQuery request, CancellationToken ct)
    {
        return await repository.Addresses(request.CustomerId, ct);
    }
}
