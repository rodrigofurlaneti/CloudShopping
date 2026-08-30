using CloudShopping.Application;
using CloudShopping.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers e serialização JSON (enums como strings)
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

    c.AddSecurityDefinition("X-Tenant-Id", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Identificador do Tenant (multi-tenant). Ex: 1"
    });
});

// 3. Camadas da aplicação (Application + Infrastructure)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 4. Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
