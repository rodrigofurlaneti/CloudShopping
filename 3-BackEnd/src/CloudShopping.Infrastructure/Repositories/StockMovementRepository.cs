using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class StockMovementRepository : IStockMovementRepository
    {
        private readonly AppDbContext _context;

        public StockMovementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.StockMovements.FirstOrDefaultAsync(sm => sm.Id == id, cancellationToken);
        }

        public async Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
        {
            await _context.StockMovements.AddAsync(stockMovement, cancellationToken);
        }

        public void Update(StockMovement stockMovement)
        {
            _context.StockMovements.Update(stockMovement);
        }

        public void Remove(StockMovement stockMovement)
        {
            _context.StockMovements.Remove(stockMovement);
        }

        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.StockMovements
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
