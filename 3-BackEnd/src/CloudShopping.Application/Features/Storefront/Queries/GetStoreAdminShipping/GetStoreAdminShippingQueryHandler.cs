using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreAdminShipping;
public sealed class GetStoreAdminShippingQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreAdminShippingQuery, Result<IReadOnlyList<StoreShippingView>>>
{
    public Task<Result<IReadOnlyList<StoreShippingView>>> Handle(GetStoreAdminShippingQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<StoreShippingView>> ExecuteAsync(GetStoreAdminShippingQuery request, CancellationToken ct)
    {
        return await repository.Shipping(null, ct);
    }
}
