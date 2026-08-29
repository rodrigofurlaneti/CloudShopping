using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders
{
    public sealed record GetPaginatedTenantOrdersQuery(
        int Page = 1,
        int PageSize = 10,
        OrderStatus? StatusFilter = null
    ) : IRequest<Result<PagedResult<OrderSummaryResponse>>>;
}
