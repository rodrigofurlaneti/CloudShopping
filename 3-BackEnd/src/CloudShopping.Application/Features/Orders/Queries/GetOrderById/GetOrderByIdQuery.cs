using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed record GetOrderByIdQuery(
        int OrderId,
        int CustomerId) : IRequest<Result<OrderDetailsViewModel>>;
}