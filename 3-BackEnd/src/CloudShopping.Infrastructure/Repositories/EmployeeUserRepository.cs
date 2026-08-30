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
    public sealed class EmployeeUserRepository : IEmployeeUserRepository
    {
        protected readonly AppDbContext _context;

        public EmployeeUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeUser?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EmployeeUser>().FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(EmployeeUser entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<EmployeeUser>().AddAsync(entity, cancellationToken);
        }

        public void Update(EmployeeUser entity)
        {
            _context.Set<EmployeeUser>().Update(entity);
        }

        public void Remove(EmployeeUser entity)
        {
            _context.Set<EmployeeUser>().Remove(entity);
        }

        public async Task<EmployeeUser?> GetByUsernameAsync(int tenantId, string username, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EmployeeUser>()
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Username == username, cancellationToken);
        }

        public async Task<IEnumerable<EmployeeUser>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<EmployeeUser>()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }
    }
}