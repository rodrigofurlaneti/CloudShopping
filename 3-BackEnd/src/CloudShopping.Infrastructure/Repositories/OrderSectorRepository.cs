using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class OrderSectorRepository : IOrderSectorRepository
    {
        private readonly AppDbContext _context;

        public OrderSectorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderSector?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.OrderSectors.FirstOrDefaultAsync(os => os.Id == id, cancellationToken);
        }

        public async Task AddAsync(OrderSector entity, CancellationToken cancellationToken = default)
        {
            await _context.OrderSectors.AddAsync(entity, cancellationToken);
        }

        public void Update(OrderSector entity)
        {
            _context.OrderSectors.Update(entity);
        }

        public void Remove(OrderSector entity)
        {
            _context.OrderSectors.Remove(entity);
        }
    }
}
