using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.StartOrderProcessing
{
    public sealed record StartOrderProcessingCommand(int OrderId) : IRequest<Result>;
}