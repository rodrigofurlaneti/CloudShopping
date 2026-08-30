using CloudShopping.Domain.Entities.Orders;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface IOrderStateHistoryRepository : IRepository<OrderStateHistory, int>
    {
    }
}
