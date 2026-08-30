using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly AppDbContext _context;

        public OrderStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderStatus?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStatuses.FirstOrDefaultAsync(os => os.Id == id, cancellationToken);
        }

        public async Task AddAsync(OrderStatus entity, CancellationToken cancellationToken = default)
        {
            await _context.OrderStatuses.AddAsync(entity, cancellationToken);
        }

        public void Update(OrderStatus entity)
        {
            _context.OrderStatuses.Update(entity);
        }

        public void Remove(OrderStatus entity)
        {
            _context.OrderStatuses.Remove(entity);
        }
    }
}
