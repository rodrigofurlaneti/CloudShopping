using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Caching;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Repositories;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using StackExchange.Redis;
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

        // Cache Redis (tarefa de cache full-stack): ConnectionStrings:Redis é opcional de
        // propósito — sem ela (ou se a conexão inicial falhar), a aplicação sobe normalmente
        // usando NullCacheService (no-op), sem cache mas também sem quebrar nada.
        // Configure ConnectionStrings__Redis (env var) em produção; em desenvolvimento o
        // padrão é localhost:6379 (ver appsettings.Development.json).
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var redisInstanceName = configuration["Redis:InstanceName"] ?? "cloudshopping:";
        services.AddSingleton<ICacheService>(sp =>
        {
            var startupLogger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("CloudShopping.Infrastructure.Caching.Redis");
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                startupLogger.LogWarning("ConnectionStrings:Redis não configurada. Cache desabilitado (NullCacheService).");
                return new NullCacheService();
            }
            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 3000;
                var multiplexer = ConnectionMultiplexer.Connect(options);
                TryEnableAllKeysLru(multiplexer, startupLogger);
                return new RedisCacheService(multiplexer, redisInstanceName, sp.GetRequiredService<ILogger<RedisCacheService>>());
            }
            catch (Exception ex)
            {
                startupLogger.LogWarning(ex, "Não foi possível conectar ao Redis ({ConnectionString}). Cache desabilitado (NullCacheService); a aplicação segue funcionando normalmente.", redisConnectionString);
                return new NullCacheService();
            }
        });

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

    // Best-effort: define a política de despejo allkeys-lru exigida pelo DoD da tarefa de
    // cache. Um Redis gerenciado (ex.: Azure Cache Basic/Standard) costuma recusar
    // CONFIG SET — nesse caso a política deve ser ajustada manualmente no provedor; a
    // falha aqui nunca deve impedir a aplicação de subir.
    private static void TryEnableAllKeysLru(IConnectionMultiplexer multiplexer, ILogger logger)
    {
        try
        {
            foreach (var endpoint in multiplexer.GetEndPoints())
            {
                var server = multiplexer.GetServer(endpoint);
                if (server.IsReplica) continue;
                server.ConfigSet("maxmemory-policy", "allkeys-lru");
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Não foi possível definir maxmemory-policy=allkeys-lru via CONFIG SET (comum em Redis gerenciado). Configure manualmente no provedor se necessário.");
        }
    }
}
