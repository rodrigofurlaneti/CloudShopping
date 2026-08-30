using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.GenerateShippingLabel
{
    public sealed record GenerateShippingLabelCommand(int OrderId) : IRequest<Result>;
}