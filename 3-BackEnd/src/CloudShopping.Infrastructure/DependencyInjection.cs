using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CloudShopping.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            return services;
        }
    }
}
