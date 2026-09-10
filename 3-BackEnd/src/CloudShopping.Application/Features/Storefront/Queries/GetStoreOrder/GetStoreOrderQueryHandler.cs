using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreOrder;
public sealed class GetStoreOrderQueryHandler(IStoreCommerce commerce) : IRequestHandler<GetStoreOrderQuery, PlacedOrder>
{
    public async Task<PlacedOrder> Handle(GetStoreOrderQuery request, CancellationToken ct)
    {
        return await commerce.GetOrder(request.CustomerId, request.Id, ct);
    }
}
