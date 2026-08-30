using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders
{
    public sealed record GetTenantOrdersQuery(
        int? OrderStatusId = null,
        int Page = 1,
        int PageSize = 20) : IRequest<Result<PagedList<OrderAdminViewModel>>>;
}