using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace CloudShopping.Infrastructure.Repositories;

public sealed class SessionAccounts(AppDbContext db) : ISessionAccounts
{
    public Task<EmployeeUser?> EmployeeLogin(string username, CancellationToken ct) => db.Set<EmployeeUser>().SingleOrDefaultAsync(x => x.Username == username && x.IsActive, ct);
    public Task<bool> EmployeeActive(int id, CancellationToken ct) => db.Set<Employee>().AnyAsync(x => x.Id == id && x.IsActive, ct);
    public Task<string[]> Permissions(int userId, CancellationToken ct) => new StorePermissions(db).ForUser(userId, ct);
    public Task<Customer?> CustomerByEmail(string email, CancellationToken ct) => db.Customers.SingleOrDefaultAsync(x => x.Email == email, ct);
    public Task<Customer?> CustomerById(int id, CancellationToken ct) => db.Customers.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> EmailUsed(string email, int exceptCustomer, CancellationToken ct) => db.Customers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == db.CurrentTenantId && x.Email == email && x.Id != exceptCustomer, ct);
    public void AddCustomer(Customer customer) => db.Add(customer);
    public Task<Cart?> Cart(int customerId, CancellationToken ct) => db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.CustomerId == customerId, ct);
    public void AddCart(Cart cart) => db.Add(cart);
    public Task<Product?> Product(int id, CancellationToken ct) => db.Products.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task Save(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task<ISessionTransaction> Begin(CancellationToken ct) => new Transaction(await db.Database.BeginTransactionAsync(ct));
    private sealed class Transaction(IDbContextTransaction transaction) : ISessionTransaction
    {
        public Task Commit(CancellationToken ct) => transaction.CommitAsync(ct);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
