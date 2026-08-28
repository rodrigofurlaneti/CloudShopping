using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface ICartRepository : IRepository<Cart, int>
    {
        Task<(IEnumerable<Cart> Items, int TotalCount)> GetPaginatedAsync(int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
        Task<Cart?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
        Task<Cart?> GetBySessionTokenAsync(Guid sessionToken, CancellationToken cancellationToken = default);
    }
}
