using CloudShopping.Domain.Entities.Backoffice;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProfileRepository : IRepository<Profile, int>
    {
        Task<Profile?> GetByNameAsync(int tenantId, string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Profile>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}