using CloudShopping.Domain.Entities.Orders;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IOrderStatusRepository : IRepository<OrderStatus, int>
    {
    }
}
