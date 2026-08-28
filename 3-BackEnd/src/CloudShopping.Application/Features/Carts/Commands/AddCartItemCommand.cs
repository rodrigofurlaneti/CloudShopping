using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Carts.Commands
{
    public sealed record AddCartItemCommand(int CartId, int ProductId, int Quantity) : IRequest<Result>;
}
