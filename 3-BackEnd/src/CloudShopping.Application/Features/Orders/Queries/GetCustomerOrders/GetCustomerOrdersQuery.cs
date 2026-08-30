using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders
{
    public sealed record GetCustomerOrdersQuery(
        int CustomerId,
        int Page = 1,
        int PageSize = 10) : IRequest<Result<PagedList<OrderSummaryViewModel>>>;
}