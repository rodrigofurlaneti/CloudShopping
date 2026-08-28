using CloudShopping.Domain.Entities.Tenants;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
