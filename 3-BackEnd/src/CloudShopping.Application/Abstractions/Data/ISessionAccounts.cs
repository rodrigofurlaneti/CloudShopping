using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Products;
namespace CloudShopping.Application.Abstractions.Data;

public interface ISessionTransaction : IAsyncDisposable { Task Commit(CancellationToken ct); }
public interface ISessionAccounts
{
    Task<EmployeeUser?> EmployeeLogin(string username, CancellationToken ct);
    Task<bool> EmployeeActive(int id, CancellationToken ct);
    Task<string[]> Permissions(int userId, CancellationToken ct);
    Task<Customer?> CustomerByEmail(string email, CancellationToken ct);
    Task<Customer?> CustomerById(int id, CancellationToken ct);
    Task<bool> EmailUsed(string email, int exceptCustomer, CancellationToken ct);
    void AddCustomer(Customer customer);
    Task<Cart?> Cart(int customerId, CancellationToken ct);
    void AddCart(Cart cart);
    Task<Product?> Product(int id, CancellationToken ct);
    Task Save(CancellationToken ct);
    Task<ISessionTransaction> Begin(CancellationToken ct);
}
