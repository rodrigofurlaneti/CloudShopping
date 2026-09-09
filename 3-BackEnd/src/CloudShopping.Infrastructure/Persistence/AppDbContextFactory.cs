using CloudShopping.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CloudShopping.Infrastructure.Persistence
{
    /// <summary>
    /// Fábrica usada pelas ferramentas de design-time do EF Core (ex: "dotnet ef migrations add"),
    /// que não têm acesso ao pipeline de injeção de dependência da aplicação em tempo de execução.
    /// </summary>
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var apiSettingsPath = Path.Combine(basePath, "..", "CloudShopping.Api");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.Exists(apiSettingsPath) ? apiSettingsPath : basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 43)));

            var tenantProvider = new TenantProvider(new HttpContextAccessor());

            return new AppDbContext(optionsBuilder.Options, tenantProvider);
        }
    }
}
