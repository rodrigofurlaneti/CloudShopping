using CloudShopping.Domain.Entities.Store;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IStoreBannerRepository : IRepository<StoreBanner, int>
    {
        Task<IEnumerable<StoreBanner>> GetAllByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    }
}