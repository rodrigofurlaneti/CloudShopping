using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _context.Carts.AddAsync(cart, cancellationToken);
        }

        public void Update(Cart cart)
        {
            _context.Carts.Update(cart);
        }

        public void Remove(Cart cart)
        {
            _context.Carts.Remove(cart);
        }

        public async Task<Cart?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        }

        public async Task<Cart?> GetBySessionTokenAsync(Guid sessionToken, CancellationToken cancellationToken = default)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionToken == sessionToken, cancellationToken);
        }
    }
}
