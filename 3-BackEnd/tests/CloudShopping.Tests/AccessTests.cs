using CloudShopping.Domain.Entities.Security;
using CloudShopping.Application.Features.AccountSecurity;
using CloudShopping.Application.Features.Access;
using CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
using CloudShopping.Infrastructure.Repositories;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed partial class CommerceTests
{
    private async Task<string> AccountSession(AppDbContext db, string password = "Current-Password-2026!")
    {
        var customer = await db.Customers.SingleAsync(x => x.Id == customerId);
        customer.SetPassword(new PasswordHasher().Hash(password));
        var session = AuthSession.Create(1, customerId, "Customer", AccountSecurity.Stamp(customer.PasswordHash!), DateTime.UtcNow.AddHours(1));
        db.Add(session); await db.SaveChangesAsync(); return session.Id;
    }
    [Fact]
    public async Task Password_change_revokes_all_sessions_and_rejects_old_session()
    {
        await using var db = Db(1); var sid = await AccountSession(db);
        var second = AuthSession.Create(1, customerId, "Customer", (await db.Set<AuthSession>().SingleAsync()).CredentialStamp, DateTime.UtcNow.AddHours(1));
        db.Add(second); await db.SaveChangesAsync();
        var security = new AccountSecurity(new AccountSecurityRepository(db), new UnitOfWork(db), new PasswordHasher());
        await Assert.ThrowsAsync<ArgumentException>(() => security.ChangePassword(customerId, "Customer", sid, "wrong", "Changed-Password-2026!", default));
        Assert.Equal(2, await db.Set<AuthSession>().CountAsync(x => x.RevokedAt == null));
        await security.ChangePassword(customerId, "Customer", sid, "Current-Password-2026!", "Changed-Password-2026!", default);
        db.ChangeTracker.Clear(); Assert.True(new PasswordHasher().Verify("Changed-Password-2026!", (await db.Customers.SingleAsync()).PasswordHash!));
        Assert.Equal(0, await db.Set<AuthSession>().CountAsync(x => x.RevokedAt == null));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => security.Sessions(customerId, "Customer", sid, default));
    }
    [Fact]
    public async Task Session_revocation_is_owner_kind_and_tenant_scoped()
    {
        await using var db = Db(1); var sid = await AccountSession(db);
        var foreign = AuthSession.Create(1, customerId + 10, "Customer", "", DateTime.UtcNow.AddHours(1));
        var staff = AuthSession.Create(1, customerId, "Administrator", "", DateTime.UtcNow.AddHours(1));
        db.AddRange(foreign, staff); await db.SaveChangesAsync(); var security = new AccountSecurity(new AccountSecurityRepository(db), new UnitOfWork(db), new PasswordHasher());
        Assert.Single(await security.Sessions(customerId, "Customer", sid, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => security.Revoke(customerId, "Customer", sid, foreign.Id, default));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => security.Revoke(customerId, "Customer", sid, staff.Id, default));
        await using var other = Db(2);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new AccountSecurity(new AccountSecurityRepository(other), new UnitOfWork(other), new PasswordHasher()).Revoke(customerId, "Customer", sid, null, default));
        await security.Revoke(customerId, "Customer", sid, null, default);
        Assert.Equal(3, await db.Set<AuthSession>().CountAsync(x => x.RevokedAt == null));
    }
    [Fact]
    public async Task Profile_permissions_update_live_with_audit_and_stale_write_protection()
    {
        await using var db = Db(1);
        var employee = Employee.Create(1, "Operador", "52998224725", "operator@example.test", null, DateTime.UtcNow, null, null);
        var adminProfile = Profile.Create(1, "Administrador Geral"); var profile = Profile.Create(1, "Consulta");
        db.AddRange(employee, adminProfile, profile); await db.SaveChangesAsync();
        var admin = EmployeeUser.Create(1, employee.Id, "test-admin", "test-hash"); var staff = EmployeeUser.Create(1, employee.Id, "test-staff", "test-hash");
        db.AddRange(admin, staff); await db.SaveChangesAsync();
        db.AddRange(ProfileUser.Create(1, adminProfile.Id, admin.Id), ProfileUser.Create(1, profile.Id, staff.Id)); await db.SaveChangesAsync();
        var repository = new AccessRepository(db);
        var handler = new ReplaceProfilePermissionsCommandHandler(repository);
        Assert.True((await handler.Handle(new(admin.Id, profile.Id, [], ["catalog.read"]), default)).IsSuccess);
        Assert.Equal(new[] { "catalog.read" }, await repository.ForUser(staff.Id, default));
        Assert.False(PermissionPolicy.Allows(await repository.ForUser(staff.Id, default), "stock.write"));
        Assert.Equal("Command.Forbidden", (await handler.Handle(new(staff.Id, profile.Id, ["catalog.read"], ["finance.read"]), default)).Error.Code);
        Assert.Equal("Command.Conflict", (await handler.Handle(new(admin.Id, profile.Id, [], ["finance.read"]), default)).Error.Code);
        Assert.True((await handler.Handle(new(admin.Id, profile.Id, ["catalog.read"], []), default)).IsSuccess);
        Assert.Empty(await repository.ForUser(staff.Id, default)); Assert.Equal(2, await db.Set<AccessChange>().CountAsync());
        await using var other = Db(2); Assert.Empty(await new AccessRepository(other).ForUser(staff.Id, default));
        Assert.Empty(await other.Set<AccessChange>().ToListAsync());
    }
}

public sealed class AccessPolicyTests
{
    [Theory]
    [InlineData("Products", "AdjustInventory", false, "stock.write")]
    [InlineData("Asaas", "Refund", false, "finance.refund")]
    [InlineData("Asaas", "Configure", false, "settings.write")]
    [InlineData("Backoffice", "CreateProfile", false, "*")]
    [InlineData("NewUnknownController", "List", true, "*")]
    public void Sensitive_operations_and_unknown_controllers_do_not_inherit_read_access(string controller, string action, bool read, string required)
    {
        Assert.Contains(required, AccessRequirements.For(controller, action, read));
        Assert.False(AccessRequirements.For(controller, action, read).All(x => PermissionPolicy.Allows(["catalog.read", "finance.read"], x)));
    }
}
