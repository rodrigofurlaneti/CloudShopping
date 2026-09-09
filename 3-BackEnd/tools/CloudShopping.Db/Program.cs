using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection explicitamente.");
if (args.Length < 1) throw new ArgumentException("Informe o diretório de migrations.");
await SchemaMigrator.Apply(connection, Path.GetFullPath(args[0]));
Console.WriteLine("Migrações aplicadas/verificadas.");
if (args.Contains("--demo"))
{
    var settings = new MySqlConnectionStringBuilder(connection);
    if (settings.Server != "127.0.0.1" || settings.Port != 33077 || settings.Database != "cloudshopping_dev")
        throw new InvalidOperationException("Demonstração somente em 127.0.0.1:33077/cloudshopping_dev.");
    var password = Environment.GetEnvironmentVariable("CLOUDSHOPPING_DEMO_PASSWORD")
        ?? throw new InvalidOperationException("Informe CLOUDSHOPPING_DEMO_PASSWORD (mínimo 12 caracteres).");
    if (password.Length < 12) throw new ArgumentException("Senha de demonstração muito curta.");
    await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseMySql(connection, new MySqlServerVersion(new Version(8,0,43))).Options, new DemoTenant());
    db.IsProvisioning = true;
    if (!await db.Tenants.IgnoreQueryFilters().AnyAsync())
    {
        db.Add(Tenant.Create("Loja de demonstração", "localhost"));
        db.Add(Tenant.Create("Segunda loja de teste", "second.localhost"));
        await db.SaveChangesAsync();
        var department = Department.CreateForTenant(1, "Tecnologia", "tecnologia"); db.Add(department);
        var employee = Employee.Create(1, "Administrador de teste", "52998224725", "admin@example.test", null, DateTime.UtcNow, null, null);
        db.Add(employee); var profile = Profile.Create(1, "Administrador Geral"); db.Add(profile); await db.SaveChangesAsync();
        var admin = EmployeeUser.Create(1, employee.Id, "admin", BCrypt.Net.BCrypt.HashPassword(password)); db.Add(admin); await db.SaveChangesAsync();
        db.Add(ProfileUser.Create(1, profile.Id, admin.Id));
        var product = Product.Create(1, department.Id, "DEMO-001", "Smartphone de demonstração", 1299.90m, 10); db.Add(product);
        db.Add(new ShippingOption { TenantId=1, Name="Entrega de demonstração", Amount=15m, PostalCodePrefix="01", EstimatedDays=2 });
        await db.SaveChangesAsync();
        Console.WriteLine("Dados sintéticos criados. Usuário administrativo: admin.");
    }
}
sealed class DemoTenant : ITenantProvider { public int GetTenantId() => 1; }
