using CloudShopping.Application;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure;
using CloudShopping.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

public sealed class DependencyInjectionTests
{
    [Fact]
    public async Task All_request_handlers_can_be_constructed_in_a_scope()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
            Args = []
        });
        // Construction only: no host, workers, database or payment requests are started.
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            "Server=127.0.0.1;Port=33077;User ID=root;Database=cloudshopping_di_test;SslMode=None";
        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });
        builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var handlers = builder.Services.Select(x => x.ServiceType)
            .Where(t => t.IsGenericType && !t.ContainsGenericParameters &&
                (t.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                 t.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
            .Distinct().ToArray();
        Assert.NotEmpty(handlers);

        await using var app = builder.Build();
        await using var scope = app.Services.CreateAsyncScope();
        foreach (var handler in handlers)
            Assert.NotNull(scope.ServiceProvider.GetRequiredService(handler));
        Assert.Same(scope.ServiceProvider.GetRequiredService<StoreCommerceService>(),
            scope.ServiceProvider.GetRequiredService<IStoreCommerce>());
    }
}
