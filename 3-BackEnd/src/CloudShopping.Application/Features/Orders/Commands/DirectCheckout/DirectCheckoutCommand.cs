using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed record DirectCheckoutAddressDto(
        AddressType AddressTypeId,
        string Street,
        string Number,
        string? Neighborhood,
        string City,
        string State,
        string ZipCode);

    public sealed record DirectCheckoutItemDto(int ProductId, int Quantity);

    public sealed record DirectCheckoutCommand(
        int CustomerId,
        List<DirectCheckoutItemDto> Items,
        DirectCheckoutAddressDto DeliveryAddress) : IRequest<Result<int>>;
}
