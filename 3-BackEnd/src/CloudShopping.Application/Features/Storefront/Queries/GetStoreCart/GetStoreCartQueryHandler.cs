using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreCart;
public sealed class GetStoreCartQueryHandler(IStoreCommerce commerce) : IRequestHandler<GetStoreCartQuery, CartView>
{
    public async Task<CartView> Handle(GetStoreCartQuery request, CancellationToken ct)
    {
        return await commerce.ViewCart(request.CustomerId, ct);
    }
}
