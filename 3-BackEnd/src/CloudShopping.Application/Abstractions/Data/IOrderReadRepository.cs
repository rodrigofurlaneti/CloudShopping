using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline;
using CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders;
using CloudShopping.Application.Features.Orders.Queries.GetTenantOrders;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace CloudShopping.Application.Abstractions.Data;
public interface IOrderReadRepository
{
    Task<Result<PagedList<OrderAdminViewModel>>> GetTenantOrdersAsync(GetTenantOrdersQuery request, CancellationToken cancellationToken);
    Task<Result<PagedResult<OrderSummaryResponse>>> GetPaginatedTenantOrdersAsync(GetPaginatedTenantOrdersQuery request, CancellationToken cancellationToken);
    Task<Result<IEnumerable<OrderTimelineViewModel>>> GetOrderTimelineAsync(GetOrderTimelineQuery request, CancellationToken cancellationToken);
}
