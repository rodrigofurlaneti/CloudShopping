using CloudShopping.Domain.Entities.Customers;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface ICustomerRepository : IRepository<Customer, int> 
    {
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetPaginatedAsync(int tenantId, int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetInactiveGuestsAsync(int daysInactive, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(int tenantId, string email, CancellationToken cancellationToken = default);
    }
}
