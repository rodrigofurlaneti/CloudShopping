using System.Text;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
namespace CloudShopping.Application.Features.Sessions;

public sealed record SessionCaller(int Id, string? Role, string? SessionId);
public sealed record SessionLogin(int Id, string Name, string Role, bool IsGuest, StoredSession? Session);

public sealed class SessionUseCases(ISessionAccounts accounts, SessionLifecycle sessions, IPasswordHasher hasher, ITenantProvider tenant, TimeProvider clock)
{
    public Task<string[]> Permissions(SessionCaller caller, CancellationToken ct) => caller.Role == "Administrator" ? accounts.Permissions(caller.Id, ct) : Task.FromResult(Array.Empty<string>());
    public Task Logout(SessionCaller caller, CancellationToken ct) => sessions.Revoke(tenant.GetTenantId(), caller.SessionId, ct);

    public async Task<SessionLogin?> AdminLogin(SessionCaller caller, string username, string password, CancellationToken ct)
    {
        var employee = await accounts.EmployeeLogin(username.Trim(), ct);
        if (employee == null || !hasher.Verify(password, employee.PasswordHash)) return null;
        if (!await accounts.EmployeeActive(employee.EmployeeId, ct) || (await accounts.Permissions(employee.Id, ct)).Length == 0) throw new UnauthorizedAccessException("Acesso administrativo não autorizado.");
        return await Issue(caller, employee.Id, employee.Username, "Administrator", employee.PasswordHash, false, ct);
    }

    public async Task<SessionLogin> Guest(SessionCaller caller, CancellationToken ct)
    {
        if (caller.Role == "Administrator") throw new InvalidOperationException("Saia da sessão administrativa para comprar.");
        if (caller.Role == "Customer") return new(caller.Id, "Visitante", "Customer", true, null);
        var customer = Customer.CreateGuest(tenant.GetTenantId()); accounts.AddCustomer(customer); await accounts.Save(ct);
        return await Issue(caller, customer.Id, "Visitante", "Customer", customer.SessionToken.ToString(), true, ct);
    }

    public async Task<SessionLogin> Register(SessionCaller caller, string email, string password, CancellationToken ct)
    {
        if (caller.Role == "Administrator") throw new UnauthorizedAccessException();
        if (password.Length < 12 || Encoding.UTF8.GetByteCount(password) > 72) throw new ArgumentException("Senha deve ter no mínimo 12 caracteres e no máximo 72 bytes.");
        email = email.Trim().ToLowerInvariant();
        if (await accounts.EmailUsed(email, caller.Id, ct)) throw new InvalidOperationException("Cadastro indisponível para este email. Entre com sua conta.");
        var customer = caller.Role == "Customer" ? await accounts.CustomerById(caller.Id, ct) ?? throw new UnauthorizedAccessException() : Customer.CreateGuest(tenant.GetTenantId());
        if (customer.PasswordHash != null) throw new InvalidOperationException("A conta já está cadastrada.");
        customer.ChangeEmail(email); customer.SetPassword(hasher.Hash(password));
        if (customer.Id == 0) accounts.AddCustomer(customer);
        await accounts.Save(ct);
        return await Issue(caller, customer.Id, email, "Customer", customer.PasswordHash!, false, ct);
    }

    public async Task<SessionLogin?> CustomerLogin(SessionCaller caller, string email, string password, CancellationToken ct)
    {
        if (caller.Role == "Administrator") throw new UnauthorizedAccessException();
        email = email.Trim().ToLowerInvariant();
        var customer = await accounts.CustomerByEmail(email, ct);
        if (customer?.PasswordHash == null || !hasher.Verify(password, customer.PasswordHash)) return null;
        if (caller.Role == "Customer" && caller.Id != customer.Id) await MergeGuestCart(caller.Id, customer.Id, ct);
        return await Issue(caller, customer.Id, email, "Customer", customer.PasswordHash, false, ct);
    }

    private async Task MergeGuestCart(int sourceCustomer, int destinationCustomer, CancellationToken ct)
    {
        await using var transaction = await accounts.Begin(ct);
        var guest = await accounts.CustomerById(sourceCustomer, ct) ?? throw new UnauthorizedAccessException();
        if (guest.PasswordHash == null)
        {
            var source = await accounts.Cart(sourceCustomer, ct);
            if (source != null && source.ExpiresAt > clock.GetUtcNow().UtcDateTime && source.Items.Count > 0)
            {
                var destination = await accounts.Cart(destinationCustomer, ct);
                if (destination == null) { destination = Cart.Create(destinationCustomer); accounts.AddCart(destination); }
                else if (destination.ExpiresAt <= clock.GetUtcNow().UtcDateTime) destination.Clear();
                foreach (var item in source.Items)
                {
                    var product = await accounts.Product(item.ProductId, ct);
                    if (product == null) continue;
                    var quantity = destination.Items.SingleOrDefault(x => x.ProductId == product.Id)?.Quantity ?? 0;
                    var addition = Math.Min(item.Quantity, 999 - quantity);
                    if (addition > 0) destination.AddOrUpdateItem(product.Id, addition, product.Price);
                }
                source.Clear(); await accounts.Save(ct);
            }
        }
        await transaction.Commit(ct);
    }

    private async Task<SessionLogin> Issue(SessionCaller caller, int id, string name, string role, string credential, bool guest, CancellationToken ct)
    {
        await Logout(caller, ct);
        var session = await sessions.Create(tenant.GetTenantId(), id, role, credential, ct);
        return new(id, name, role, guest, session);
    }
}
