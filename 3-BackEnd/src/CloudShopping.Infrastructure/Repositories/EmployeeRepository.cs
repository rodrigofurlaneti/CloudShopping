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
    public sealed class EmployeeRepository : IEmployeeRepository
    {
        protected readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(Employee entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<Employee>().AddAsync(entity, cancellationToken);
        }

        public void Update(Employee entity)
        {
            _context.Set<Employee>().Update(entity);
        }

        public void Remove(Employee entity)
        {
            _context.Set<Employee>().Remove(entity);
        }

        public async Task<Employee?> GetByCpfAsync(int tenantId, string cpf, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Cpf == cpf, cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Employee>()
                .Where(e => e.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }
    }
}