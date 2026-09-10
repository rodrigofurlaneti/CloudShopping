using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.ConfirmStoreCheckout;
public sealed class ConfirmStoreCheckoutCommandHandler(IStoreCommerce commerce) : IRequestHandler<ConfirmStoreCheckoutCommand, Result<PlacedOrder>>
{
    public Task<Result<PlacedOrder>> Handle(ConfirmStoreCheckoutCommand request, CancellationToken ct)
        => CommandExecution.Run<PlacedOrder>(async () =>
    {
        return await commerce.Confirm(request.CustomerId, request.Key, request.Token, ct);
    
    });
}
