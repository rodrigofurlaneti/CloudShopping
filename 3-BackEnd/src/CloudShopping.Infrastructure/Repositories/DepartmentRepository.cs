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
    internal sealed class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _dbContext;

        public DepartmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Department>()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task AddAsync(Department entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<Department>().AddAsync(entity, cancellationToken);
        }

        public void Update(Department entity)
        {
            _dbContext.Set<Department>().Update(entity);
        }

        public void Remove(Department entity)
        {
            _dbContext.Set<Department>().Remove(entity);
        }

        public async Task<IEnumerable<Department>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Department>()
                .Where(d => d.TenantId == tenantId || d.TenantId == null)
                .OrderBy(d => d.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SlugExistsAsync(int tenantId, string slug, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Department>()
                .AnyAsync(d => (d.TenantId == tenantId || d.TenantId == null) && d.Slug == slug, cancellationToken);
        }
    }
}