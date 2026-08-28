using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.OrderAddress)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public void Update(Order order)
        {
            _context.Orders.Update(order);
        }

        public void Remove(Order order)
        {
            _context.Orders.Remove(order);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.OrderAddress)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPaginatedByTenantAsync(
            int tenantId,
            int page,
            int pageSize,
            OrderStatus? statusFilter,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.OrderAddress)
                .Where(o => o.TenantId == tenantId)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(o => o.OrderStatusId == statusFilter.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
