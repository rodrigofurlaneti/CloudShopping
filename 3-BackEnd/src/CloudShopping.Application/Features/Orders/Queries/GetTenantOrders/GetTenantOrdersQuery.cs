using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders
{
    public sealed record GetTenantOrdersQuery(int TenantId, int? OrderStatusId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedList<OrderAdminViewModel>>>;
}
