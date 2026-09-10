using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreCart;
public sealed class GetStoreCartQueryHandler(IStoreCommerce commerce) : IRequestHandler<GetStoreCartQuery, Result<CartView>>
{
    public Task<Result<CartView>> Handle(GetStoreCartQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<CartView> ExecuteAsync(GetStoreCartQuery request, CancellationToken ct)
    {
        return await commerce.ViewCart(request.CustomerId, ct);
    }
}
