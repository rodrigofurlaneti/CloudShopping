using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Commands.AddStoreAddress;
public sealed class AddStoreAddressCommandHandler(IStorefrontRepository repository) : IRequestHandler<AddStoreAddressCommand, Result<int>>
{
    public Task<Result<int>> Handle(AddStoreAddressCommand request, CancellationToken ct)
        => CommandExecution.Run<int>(async () =>
    {
        var address = Address.Create(request.CustomerId, AddressType.Shipping, request.Street, request.Number, request.Neighborhood, request.City, request.State, request.ZipCode, false);
        await repository.AddAddress(address, ct); await repository.Save(ct); return address.Id;
    
    });
}
