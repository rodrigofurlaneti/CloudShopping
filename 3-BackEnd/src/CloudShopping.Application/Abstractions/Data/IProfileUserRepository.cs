using CloudShopping.Domain.Entities.Backoffice;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProfileUserRepository : IRepository<ProfileUser, int>
    {
        Task<ProfileUser?> GetByProfileAndUserAsync(int tenantId, int profileId, int employeeUserId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProfileUser>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProfileUser>> GetByUserAsync(int employeeUserId, CancellationToken cancellationToken = default);
    }
}