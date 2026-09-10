using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders;
public sealed class GetPaginatedTenantOrdersQueryHandler(IOrderReadRepository repository) : IRequestHandler<GetPaginatedTenantOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>
{
    public Task<Result<PagedResult<OrderSummaryResponse>>> Handle(GetPaginatedTenantOrdersQuery request, CancellationToken cancellationToken) => repository.GetPaginatedTenantOrdersAsync(request, cancellationToken);
}
