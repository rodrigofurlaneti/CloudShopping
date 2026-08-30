using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn
{
    public sealed record RequestOrderReturnCommand(int OrderId, string Reason = "") : IRequest<Result>;
}
