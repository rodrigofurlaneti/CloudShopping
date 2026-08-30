using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders
{
    public sealed class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<PagedList<OrderSummaryViewModel>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<PagedList<OrderSummaryViewModel>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = (await _orderRepository.GetOrdersByCustomerAsync(request.CustomerId, cancellationToken)).ToList();

            var totalCount = orders.Count;
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var pageItems = orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderSummaryViewModel(o.Id, o.OrderDate, o.TotalAmount, ((OrderStatusEnum)o.OrderStatusId).ToString()))
                .ToList();

            var result = new PagedList<OrderSummaryViewModel>(pageItems, totalCount, page, pageSize);
            return Result.Success(result);
        }
    }
}
