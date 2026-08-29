using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders
{
    public sealed class GetPaginatedTenantOrdersQueryHandler
        : IRequestHandler<GetPaginatedTenantOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetPaginatedTenantOrdersQueryHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<PagedResult<OrderSummaryResponse>>> Handle(
            GetPaginatedTenantOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var (items, totalCount) = await _orderRepository.GetPaginatedByTenantAsync(
                tenantId,
                request.Page,
                request.PageSize,
                request.StatusFilter,
                cancellationToken);
            var responseItems = items.Select(o => new OrderSummaryResponse(
                o.Id,
                o.CustomerId,
                o.OrderDate,
                o.TotalAmount,
                o.OrderStatusId,
                o.OrderItems.Sum(i => i.Quantity)
            )).ToList().AsReadOnly();
            var pagedResult = new PagedResult<OrderSummaryResponse>(
                responseItems,
                totalCount,
                request.Page,
                request.PageSize
            );
            return Result.Success(pagedResult);
        }
    }
}
