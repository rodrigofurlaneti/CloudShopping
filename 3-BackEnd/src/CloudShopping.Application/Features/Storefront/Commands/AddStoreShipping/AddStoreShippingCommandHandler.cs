using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.AddStoreShipping;
public sealed class AddStoreShippingCommandHandler(IStorefrontRepository repository) : IRequestHandler<AddStoreShippingCommand, Result<int>>
{
    public Task<Result<int>> Handle(AddStoreShippingCommand request, CancellationToken ct)
        => CommandExecution.Run<int>(async () =>
    {
        return await repository.AddShipping(request.Name.Trim(), request.Amount, request.EstimatedDays, request.PostalCodePrefix, ct);
    
    });
}
