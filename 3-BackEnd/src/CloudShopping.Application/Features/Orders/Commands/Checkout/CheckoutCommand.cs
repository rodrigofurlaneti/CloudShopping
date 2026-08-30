using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.Checkout
{
    public sealed record CheckoutCommand(
        int CustomerId,
        int CartId,
        int AddressId) : IRequest<Result<int>>;
}