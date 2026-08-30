using CloudShopping.Domain.Entities.Backoffice;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IEmployeeRepository : IRepository<Employee, int>
    {
        Task<Employee?> GetByCpfAsync(int tenantId, string cpf, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}