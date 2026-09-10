using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.PreviewStoreCheckout;
public sealed class PreviewStoreCheckoutQueryHandler(IStoreCommerce commerce) : IRequestHandler<PreviewStoreCheckoutQuery, CheckoutPreview>
{
    public async Task<CheckoutPreview> Handle(PreviewStoreCheckoutQuery request, CancellationToken ct)
    {
        return await commerce.Preview(request.CustomerId, request.AddressId, request.ShippingId, ct, request.CouponCode);
    }
}
