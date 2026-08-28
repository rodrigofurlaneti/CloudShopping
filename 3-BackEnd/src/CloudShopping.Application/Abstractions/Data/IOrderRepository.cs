using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Enums;

namespace CloudShopping.Application.Abstractions.Data;

public interface IOrderRepository : IRepository<Order, int>
{
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetPaginatedByTenantAsync(
        int tenantId,
        int page,
        int pageSize,
        OrderStatus? statusFilter,
        CancellationToken cancellationToken = default);
}