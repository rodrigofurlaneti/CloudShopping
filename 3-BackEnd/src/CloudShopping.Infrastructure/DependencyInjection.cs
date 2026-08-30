using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Repositories;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Data;

namespace CloudShopping.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        services.AddScoped<IDbConnection>(sp => new MySqlConnection(connectionString));
        services.AddScoped<ISqlConnectionFactory>(sp => new SqlConnectionFactory(connectionString));
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IOrderSectorRepository, OrderSectorRepository>();
        services.AddScoped<IOrderStateHistoryRepository, OrderStateHistoryRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IStoreBannerRepository, StoreBannerRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeUserRepository, EmployeeUserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IProfileUserRepository, ProfileUserRepository>();
        return services;
    }
}