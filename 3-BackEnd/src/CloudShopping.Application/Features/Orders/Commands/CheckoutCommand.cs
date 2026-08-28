using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed record CheckoutCommand(int CustomerId, int CartId, int AddressId) : IRequest<Result<int>>;
}
