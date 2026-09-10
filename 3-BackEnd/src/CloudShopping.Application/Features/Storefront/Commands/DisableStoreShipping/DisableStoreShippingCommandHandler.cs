using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.DisableStoreShipping;
public sealed class DisableStoreShippingCommandHandler(IStorefrontRepository repository) : IRequestHandler<DisableStoreShippingCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(DisableStoreShippingCommand request, CancellationToken ct)
        => CommandExecution.Run<Unit>(async () =>
    {
        if (!await repository.DisableShipping(request.Id, ct)) throw new KeyNotFoundException(); return Unit.Value;
    
    });
}
