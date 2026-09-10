using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.PreviewStoreCheckout;
public sealed class PreviewStoreCheckoutQueryHandler(IStoreCommerce commerce) : IRequestHandler<PreviewStoreCheckoutQuery, Result<CheckoutPreview>>
{
    public Task<Result<CheckoutPreview>> Handle(PreviewStoreCheckoutQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<CheckoutPreview> ExecuteAsync(PreviewStoreCheckoutQuery request, CancellationToken ct)
    {
        return await commerce.Preview(request.CustomerId, request.AddressId, request.ShippingId, ct, request.CouponCode);
    }
}
