using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class ProfileRepository : IProfileRepository
    {
        protected readonly AppDbContext _context;

        public ProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Profile?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Profile>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(Profile entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<Profile>().AddAsync(entity, cancellationToken);
        }

        public void Update(Profile entity)
        {
            _context.Set<Profile>().Update(entity);
        }

        public void Remove(Profile entity)
        {
            _context.Set<Profile>().Remove(entity);
        }

        public async Task<Profile?> GetByNameAsync(int tenantId, string name, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Profile>()
                .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Name == name, cancellationToken);
        }

        public async Task<IEnumerable<Profile>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Profile>()
                .Where(p => p.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }
    }
}