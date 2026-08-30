using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                .Join(_context.Customers,
                    cart => cart.CustomerId,
                    customer => customer.Id,
                    (cart, customer) => new { Cart = cart, Customer = customer })
                .Where(x => x.Customer.SessionToken == sessionToken)
                .Select(x => x.Cart)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Cart> Items, int TotalCount)> GetPaginatedAsync(
            int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            // Cart não possui TenantId próprio; o isolamento é feito via o Tenant do Customer associado.
            var query = _context.Carts
                .Join(_context.Customers,
                    cart => cart.CustomerId,
                    customer => customer.Id,
                    (cart, customer) => new { Cart = cart, Customer = customer })
                .Where(x => x.Customer.TenantId == tenantId)
                .Select(x => x.Cart)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
