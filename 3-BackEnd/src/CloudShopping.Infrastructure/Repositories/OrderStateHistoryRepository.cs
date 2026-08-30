using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class OrderStateHistoryRepository : IOrderStateHistoryRepository
    {
        private readonly AppDbContext _context;

        public OrderStateHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderStateHistory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderStateHistories.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        }

        public async Task AddAsync(OrderStateHistory entity, CancellationToken cancellationToken = default)
        {
            await _context.OrderStateHistories.AddAsync(entity, cancellationToken);
        }

        public void Update(OrderStateHistory entity)
        {
            _context.OrderStateHistories.Update(entity);
        }

        public void Remove(OrderStateHistory entity)
        {
            _context.OrderStateHistories.Remove(entity);
        }
    }
}
