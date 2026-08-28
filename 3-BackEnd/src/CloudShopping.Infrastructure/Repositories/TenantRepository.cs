using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
