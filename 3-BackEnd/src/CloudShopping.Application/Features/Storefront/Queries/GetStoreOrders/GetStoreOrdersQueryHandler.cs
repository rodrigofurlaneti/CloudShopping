using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreOrders;
public sealed class GetStoreOrdersQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreOrdersQuery, Result<IReadOnlyList<StoreOrderSummary>>>
{
    public Task<Result<IReadOnlyList<StoreOrderSummary>>> Handle(GetStoreOrdersQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<StoreOrderSummary>> ExecuteAsync(GetStoreOrdersQuery request, CancellationToken ct)
    {
        return await repository.Orders(request.CustomerId, request.Page, ct);
    }
}
