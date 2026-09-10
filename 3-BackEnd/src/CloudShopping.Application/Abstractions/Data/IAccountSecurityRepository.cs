using CloudShopping.Domain.Entities.Security;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Customers;
namespace CloudShopping.Application.Abstractions.Data;
public interface IAccountSecurityRepository : IRepository<AuthSession,string>
{
    Task<AuthSession?> GetOwnedAsync(int subject,string kind,string id,CancellationToken ct);
    Task<IReadOnlyList<AuthSession>> GetOwnedSessionsAsync(int subject,string kind,bool activeOnly,CancellationToken ct);
    Task<EmployeeUser?> GetEmployeeAsync(int id,CancellationToken ct);
    Task<Customer?> GetCustomerAsync(int id,CancellationToken ct);
    Task<ISessionTransaction> BeginEditAsync(int subject,string kind,CancellationToken ct);
}
