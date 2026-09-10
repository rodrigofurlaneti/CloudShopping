using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Repositories;
using CloudShopping.Infrastructure.Payments;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 43)))
                .EnableDetailedErrors());
        services.AddScoped<IDbConnection>(sp => new MySqlConnection(connectionString));
        services.AddScoped<ISqlConnectionFactory>(sp => new SqlConnectionFactory(connectionString));
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILogTrackerRepository, LogTrackerRepository>();
        services.AddScoped<IProcessingLogWriter, ProcessingLogWriter>();
        services.AddScoped<IAccountSecurityRepository, AccountSecurityRepository>();
        services.AddScoped<IStorefrontRepository, StorefrontRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderWorkflowReadRepository, OrderWorkflowReadRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponRedemptionRepository, CouponRedemptionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<StoreCommerceService>();
        services.AddScoped<IStoreCommerce>(sp => sp.GetRequiredService<StoreCommerceService>());
        services.AddScoped<ICustomerPaymentCancellation, CustomerPaymentCancellation>();
        services.AddHttpClient<CloudShopping.Application.Abstractions.Services.IPostalCodeLookup, CloudShopping.Infrastructure.Services.ViaCepLookup>(client =>
        {
            client.BaseAddress = new Uri("https://viacep.com.br/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHttpClient<IAsaasGateway, AsaasGateway>()
            .RedactLoggedHeaders(_ => true)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddScoped<AsaasAccounts>();
        services.AddScoped<AsaasPayments>();
        services.AddScoped<IAccessRepository, AccessRepository>();
        services.AddScoped<ISessionStore, SessionStore>();
        services.AddScoped<ISessionAccounts, SessionAccounts>();
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
