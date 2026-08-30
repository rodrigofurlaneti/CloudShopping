using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public CustomerRepository(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantProvider.GetTenantId();

            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.Contacts)
                .Include(c => c.Individual)
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public void Update(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public void Remove(Customer customer)
        {
            _context.Customers.Remove(customer);
        }

        public async Task<bool> EmailExistsAsync(int tenantId, string email, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AnyAsync(c => c.TenantId == tenantId && c.Email == email, cancellationToken);
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPaginatedAsync(
            int tenantId,
            int page,
            int pageSize,
            string? searchTerm,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Customers
                .Include(c => c.Individual)
                .Include(c => c.Company)
                .Where(c => c.TenantId == tenantId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    (c.Email != null && c.Email.Contains(searchTerm)) ||
                    (c.Individual != null && c.Individual.FullName.Contains(searchTerm)) ||
                    (c.Company != null && c.Company.CompanyName.Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<IEnumerable<Customer>> GetInactiveGuestsAsync(int daysInactive, CancellationToken cancellationToken = default)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-daysInactive);

            return await _context.Customers
                .Where(c => c.CustomerTypeId == CustomerType.Guest && c.UpdatedAt < thresholdDate)
                .ToListAsync(cancellationToken);
        }
    }
}