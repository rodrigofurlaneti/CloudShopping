using CloudShopping.Domain.Entities.Backoffice;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IEmployeeUserRepository : IRepository<EmployeeUser, int>
    {
        Task<EmployeeUser?> GetByUsernameAsync(int tenantId, string username, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmployeeUser>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}