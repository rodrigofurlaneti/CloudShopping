using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services; // Exemplo de namespace do seu TenantProvider
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar Controllers e Serialização JSON (suporte a Enums como strings se desejado)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Documentação da API (Swagger / OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CloudShopping API",
        Version = "v1",
        Description = "API Multi-Tenant de E-commerce baseada em Clean Architecture e DDD."
    });
});

// 3. Configuração do Banco de Dados (MySQL / EF Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 4. Registro de Injeção de Dependências das Camadas
// (Aqui você registra seus Repositórios, UnitOfWork, Serviços de Tenant e Aplicação)
// Exemplo:
// builder.Services.AddScoped<IProductRepository, ProductRepository>();
// builder.Services.AddScoped<ITenantProvider, TenantProvider>();

var app = builder.Build();

// 5. Configuração do Pipeline HTTP Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Middleware customizado para extrair o Tenant (ex: via Header 'X-Tenant-Id' ou Domínio)
app.Use(async (context, next) =>
{
    // Exemplo rápido de leitura de Tenant via Header HTTP
    if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
        int.TryParse(tenantHeader, out var tenantId))
    {
        // Se houver um TenantProvider escopado, você pode injetá-lo ou configurá-lo aqui.
    }
    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();