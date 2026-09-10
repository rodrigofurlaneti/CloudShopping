using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using Xunit;

public sealed class ArchitectureRegressionTests
{
    [Fact]
    public void Application_does_not_reference_database_implementations()
    {
        var references = typeof(IProductRepository).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain(references, x => x != null &&
            (x.StartsWith("CloudShopping.Infrastructure") || x.StartsWith("Microsoft.EntityFrameworkCore") ||
             x.StartsWith("MySqlConnector") || x == "Dapper"));
    }

    [Fact]
    public void Permission_entities_protect_identity_and_audit_data()
    {
        Assert.Throws<ArgumentException>(() => ProfilePermission.Create(1, 1, "unknown.permission"));
        Assert.Throws<ArgumentException>(() => ProfilePermission.Create(0, 1, "catalog.read"));
        var audit = AccessChange.Create(1, 2, 3, ["orders.read", "catalog.read"], []);
        Assert.Equal("[\"catalog.read\",\"orders.read\"]", audit.BeforeJson);
        Assert.Equal("[]", audit.AfterJson);
        Assert.Equal(32, audit.Id.Length);
        Assert.All(typeof(AccessChange).GetProperties(), p => Assert.False(p.SetMethod?.IsPublic ?? false));
        Assert.All(typeof(ProfilePermission).GetProperties(), p => Assert.False(p.SetMethod?.IsPublic ?? false));
    }
}

public sealed partial class CommerceTests
{
    [Fact]
    public async Task Reading_missing_cart_does_not_create_persistent_state()
    {
        await using var db = Db(1);
        var first = await Service(db).ViewCart(customerId, default);
        var second = await Service(db).ViewCart(customerId, default);
        Assert.Empty(first.Items);
        Assert.Empty(second.Items);
        Assert.False(db.ChangeTracker.HasChanges());
        Assert.Empty(db.Carts);
    }
}
