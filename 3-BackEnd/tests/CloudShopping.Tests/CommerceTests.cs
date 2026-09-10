using CloudShopping.Domain.Exceptions;
using CloudShopping.Application.Features.Storefront.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Xunit;

public sealed partial class CommerceTests : IAsyncLifetime
{
    private readonly string database = "cloudshopping_test_" + Guid.NewGuid().ToString("N");
    private string connection = "";
    private readonly IDataProtectionProvider protection = new EphemeralDataProtectionProvider();
    private readonly IConfiguration config = new ConfigurationBuilder().Build();
    private int customerId, addressId, productId, shippingId;
    private string Migrations => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../2-MySql/migrations"));
    public async Task InitializeAsync()
    {
        var admin = new MySqlConnectionStringBuilder(Environment.GetEnvironmentVariable("CLOUDSHOPPING_TEST_CONNECTION") ??
            "Server=127.0.0.1;Port=33077;User ID=root;SslMode=None");
        if (admin.Server != "127.0.0.1" || admin.Port != 33077) throw new InvalidOperationException("Use somente o MySQL isolado 127.0.0.1:33077.");
        admin.Database = "";
        await using var conn = new MySqlConnection(admin.ConnectionString); await conn.OpenAsync();
        await new MySqlCommand($"CREATE DATABASE {database}", conn).ExecuteNonQueryAsync();
        admin.Database = database; connection = admin.ConnectionString;
        await SchemaMigrator.Apply(connection, Migrations);
        await using var db = Db(1); db.IsProvisioning = true;
        db.Add(Tenant.Create("Loja de teste A", "a.test")); db.Add(Tenant.Create("Loja de teste B", "b.test")); await db.SaveChangesAsync();
        db.IsProvisioning = false;
        var dept = Department.CreateForTenant(1, "Eletrônicos", "eletronicos"); db.Add(dept); await db.SaveChangesAsync();
        var c = Customer.CreateGuest(1); c.ChangeEmail("cliente@example.test"); c.RegisterAsB2C("52998224725", "Cliente Teste", null); db.Add(c); await db.SaveChangesAsync(); customerId = c.Id;
        var addr = Address.Create(c.Id, AddressType.Shipping, "Rua Teste", "10", "Centro", "São Paulo", "SP", "01001000", true); db.Add(addr);
        var p = Product.Create(1, dept.Id, "SKU-1", "Produto real", 100m, 1); db.Add(p);
        var ship = new ShippingOption { TenantId = 1, Name = "Entrega local", Amount = 15m, PostalCodePrefix = "01", EstimatedDays = 2 }; db.Add(ship);
        await db.SaveChangesAsync(); addressId = addr.Id; productId = p.Id; shippingId = ship.Id;
    }
    public async Task DisposeAsync()
    {
        if (string.IsNullOrEmpty(connection)) return;
        var b = new MySqlConnectionStringBuilder(connection) { Database = "" };
        await using var conn = new MySqlConnection(b.ConnectionString); await conn.OpenAsync();
        if (!database.StartsWith("cloudshopping_test_") || database.Length != 51) throw new InvalidOperationException("Nome de teste inválido.");
        await new MySqlCommand($"DROP DATABASE {database}", conn).ExecuteNonQueryAsync();
        // Each test owns a distinct schema/pool. Do not retain idle connections for dropped schemas.
        using var testPool=new MySqlConnection(connection);MySqlConnection.ClearPool(testPool);
    }
    private AppDbContext Db(int tenant) => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseMySql(connection, new MySqlServerVersion(new Version(8,0,43))).Options, new TestTenant(tenant));
    private StoreCommerceService Service(AppDbContext db) => new(db, protection, config);
    private async Task<CheckoutPreview> Prepare(AppDbContext db)
    {
        var service = Service(db); await service.ChangeCart(customerId, productId, 1, "add", default);
        return await service.Preview(customerId, addressId, shippingId, default);
    }
    [Fact]
    public async Task Checkout_persists_snapshot_history_and_replays_without_duplicate()
    {
        await using var db = Db(1); var service = Service(db); var preview = await Prepare(db); var key = Guid.NewGuid().ToString();
        var order = await service.Confirm(customerId,key,preview.Token,default);
        Assert.Equal(115m,order.TotalAmount);
        var replay = await service.Confirm(customerId,key,preview.Token,default); Assert.Equal(order.Id,replay.Id);
        Assert.Single(await db.Orders.ToListAsync()); Assert.Single(await db.OrderStateHistories.ToListAsync());
        Assert.Empty((await service.ViewCart(customerId,default)).Items);
        db.ChangeTracker.Clear(); var product = await db.Products.SingleAsync(); Assert.Equal(1,product.ReservedStock);
        product.UpdateDetails("Nome alterado",999m); await db.SaveChangesAsync();
        var stored = await db.OrderItems.SingleAsync(); Assert.Equal("Produto real",stored.ProductName); Assert.Equal(100m,stored.UnitPrice);
    }
    [Fact]
    public async Task Cancel_twice_releases_reservation_once()
    {
        await using var db = Db(1); var service = Service(db); var p = await Prepare(db);
        var order = await service.Confirm(customerId,Guid.NewGuid().ToString(),p.Token,default);
        await service.Release(order.Id,customerId,default); await service.Release(order.Id,customerId,default);
        db.ChangeTracker.Clear(); Assert.Equal(0,(await db.Products.SingleAsync()).ReservedStock);
        Assert.Equal(16,(await db.Orders.SingleAsync()).OrderStatusId);
    }
    [Fact]
    public async Task Last_unit_uses_optimistic_concurrency()
    {
        await using var a=Db(1);await using var b=Db(1);
        var p1=await a.Products.SingleAsync();var p2=await b.Products.SingleAsync();
        p1.ReserveStock(1);p2.ReserveStock(1);await a.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(()=>b.SaveChangesAsync());
        await using var verify=Db(1);Assert.Equal(1,(await verify.Products.SingleAsync()).ReservedStock);
    }
    [Fact]
    public async Task Tenant_filters_cover_parent_child_and_writes()
    {
        await using(var db=Db(1)) { await Prepare(db); }
        await using var other=Db(2);
        Assert.Empty(await other.Products.ToListAsync());Assert.Empty(await other.Addresses.ToListAsync());
        Assert.Empty(await other.Carts.ToListAsync());Assert.Empty(await other.Individuals.ToListAsync());
        other.Add(Customer.CreateGuest(1));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>other.SaveChangesAsync());
    }
    [Fact]
    public async Task Concurrent_checkouts_for_last_unit_commit_only_one_order()
    {
        CheckoutPreview first, second; int secondCustomer;
        await using (var setup=Db(1))
        {
            first=await Prepare(setup);
            var customer=Customer.CreateGuest(1);customer.ChangeEmail("second@example.test");
            customer.RegisterAsB2C("11144477735","Segundo cliente",null);setup.Add(customer);await setup.SaveChangesAsync();
            secondCustomer=customer.Id;
            var address=Address.Create(customer.Id,AddressType.Shipping,"Rua Teste","20","Centro","São Paulo","SP","01001000",true);
            setup.Add(address);await setup.SaveChangesAsync();
            await Service(setup).ChangeCart(customer.Id,productId,1,"add",default);
            second=await Service(setup).Preview(customer.Id,address.Id,shippingId,default);
        }
        await using var a=Db(1);await using var b=Db(1);
        var outcomes=await Task.WhenAll(
            Record.ExceptionAsync(()=>Service(a).Confirm(customerId,Guid.NewGuid().ToString(),first.Token,default)),
            Record.ExceptionAsync(()=>Service(b).Confirm(secondCustomer,Guid.NewGuid().ToString(),second.Token,default)));
        Assert.Single(outcomes,x=>x==null);
        Assert.IsType<CommerceConflictException>(outcomes.Single(x=>x!=null));
        await using var verify=Db(1);
        Assert.Single(await verify.Orders.ToListAsync());
        Assert.Equal(1,(await verify.Products.SingleAsync()).ReservedStock);
        Assert.Single(await verify.CartItems.ToListAsync());
    }
    [Fact]
    public async Task Quote_rejects_changed_price_and_foreign_customer()
    {
        await using var db=Db(1);var service=Service(db);var preview=await Prepare(db);
        (await db.Products.SingleAsync()).UpdateDetails("Produto real",120m);await db.SaveChangesAsync();
        await Assert.ThrowsAsync<CommerceConflictException>(()=>service.Confirm(customerId,Guid.NewGuid().ToString(),preview.Token,default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>service.Confirm(customerId+1,Guid.NewGuid().ToString(),preview.Token,default));
        Assert.Empty(await db.Orders.ToListAsync());
    }
    [Fact]
    public async Task Expiry_worker_cancels_expired_order_and_releases_only_once()
    {
        await using(var db=Db(1))
        {
            var preview=await Prepare(db);
            await Service(db).Confirm(customerId,Guid.NewGuid().ToString(),preview.Token,default);
            await db.Database.ExecuteSqlRawAsync("UPDATE orders SET ReservationExpiresAt=UTC_TIMESTAMP(6)-INTERVAL 1 MINUTE");
        }
        var services=new ServiceCollection();
        services.AddSingleton<IDataProtectionProvider>(protection);
        services.AddSingleton(new DbContextOptionsBuilder<AppDbContext>().UseMySql(connection,new MySqlServerVersion(new Version(8,0,43))).Options);
        await using var provider=services.BuildServiceProvider();
        var settings=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:DefaultConnection"]=connection }).Build();
        using var worker=new ReservationExpiryWorker(provider,settings,NullLogger<ReservationExpiryWorker>.Instance);
        await worker.RunOnce(default);await worker.RunOnce(default);
        await using var verify=Db(1);
        Assert.Equal(0,(await verify.Products.SingleAsync()).ReservedStock);
        Assert.Equal("Released",(await verify.Orders.SingleAsync()).ReservationState);
        Assert.Equal(2,await verify.OrderStateHistories.CountAsync());
    }
    [Fact]
    public async Task Migrations_are_repeatable()
    {
        await SchemaMigrator.Apply(connection,Migrations);
        await using var db=Db(1);Assert.Equal(1,await db.Products.CountAsync());
    }
    private sealed record TestTenant(int Id):ITenantProvider { public int GetTenantId()=>Id; }
}
