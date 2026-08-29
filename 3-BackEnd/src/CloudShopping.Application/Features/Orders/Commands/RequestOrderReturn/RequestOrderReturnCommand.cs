using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn
{
    public sealed record RequestOrderReturnCommand(int OrderId, int CustomerId, string Reason) : IRequest<Result>;
}
