using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed record DirectCheckoutCommand(
        int CustomerId,
        AddressDto DeliveryAddress,
        List<OrderItemDto> Items) : IRequest<Result<int>>;
}