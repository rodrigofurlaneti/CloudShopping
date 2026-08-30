using CloudShopping.Domain.Entities.Orders;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IOrderRepository : IRepository<Order, int>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    }
}