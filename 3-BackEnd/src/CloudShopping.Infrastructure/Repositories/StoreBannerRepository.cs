using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Store;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    internal sealed class StoreBannerRepository : IStoreBannerRepository
    {
        private readonly AppDbContext _dbContext;

        public StoreBannerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StoreBanner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<StoreBanner>()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task AddAsync(StoreBanner entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<StoreBanner>().AddAsync(entity, cancellationToken);
        }

        public void Update(StoreBanner entity)
        {
            _dbContext.Set<StoreBanner>().Update(entity);
        }

        public void Remove(StoreBanner entity)
        {
            _dbContext.Set<StoreBanner>().Remove(entity);
        }

        public async Task<IEnumerable<StoreBanner>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            var tenantBanners = await _dbContext.Set<StoreBanner>()
                .Where(b => b.TenantId == tenantId && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync(cancellationToken);
            if (tenantBanners.Count > 0)
            {
                return tenantBanners;
            }
            return await _dbContext.Set<StoreBanner>()
                .Where(b => b.TenantId == null && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
    }
}