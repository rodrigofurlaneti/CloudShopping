using CloudShopping.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CloudShopping.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<Features.LogTrackers.ProcessingLogRecorder>();
            services.AddScoped<Features.AccountSecurity.AccountSecurity>();
            services.AddScoped<Features.Sessions.SessionLifecycle>();
            services.AddScoped<Features.Sessions.SessionUseCases>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            services.AddValidatorsFromAssembly(assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProcessingLogBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
