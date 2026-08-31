using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class TenantRepository : ITenantRepository
    {
        private readonly AppDbContext _context;

        public TenantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            await _context.Tenants.AddAsync(tenant, cancellationToken);
        }

        public void Update(Tenant tenant)
        {
            _context.Tenants.Update(tenant);
        }

        public void Remove(Tenant tenant)
        {
            _context.Tenants.Remove(tenant);
        }

        public async Task<(IEnumerable<Tenant> Items, int TotalCount)> GetPaginatedAsync(
            int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Tenants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.CompanyName.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(t => t.CompanyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Tenant?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
        {
            var normalized = domain.Trim().ToLower();
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Domain == normalized, cancellationToken);
        }
    }
}
