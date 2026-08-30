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
    public sealed class ProfileUserRepository : IProfileUserRepository
    {
        protected readonly AppDbContext _context;

        public ProfileUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProfileUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProfileUser>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(ProfileUser entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<ProfileUser>().AddAsync(entity, cancellationToken);
        }

        public void Update(ProfileUser entity)
        {
            _context.Set<ProfileUser>().Update(entity);
        }

        public void Remove(ProfileUser entity)
        {
            _context.Set<ProfileUser>().Remove(entity);
        }

        public async Task<ProfileUser?> GetByProfileAndUserAsync(int tenantId, int profileId, int employeeUserId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProfileUser>()
                .FirstOrDefaultAsync(pu => pu.TenantId == tenantId && pu.ProfileId == profileId && pu.EmployeeUserId == employeeUserId, cancellationToken);
        }

        public async Task<IEnumerable<ProfileUser>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProfileUser>()
                .Where(pu => pu.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ProfileUser>> GetByUserAsync(int employeeUserId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProfileUser>()
                .Where(pu => pu.EmployeeUserId == employeeUserId)
                .ToListAsync(cancellationToken);
        }
    }
}