using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreOrder;
public sealed class GetStoreOrderQueryHandler(IStoreCommerce commerce) : IRequestHandler<GetStoreOrderQuery, Result<PlacedOrder>>
{
    public Task<Result<PlacedOrder>> Handle(GetStoreOrderQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<PlacedOrder> ExecuteAsync(GetStoreOrderQuery request, CancellationToken ct)
    {
        return await commerce.GetOrder(request.CustomerId, request.Id, ct);
    }
}
