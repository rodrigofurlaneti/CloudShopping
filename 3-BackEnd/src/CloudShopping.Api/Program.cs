using CloudShopping.Application;
using CloudShopping.Infrastructure;
using Microsoft.OpenApi.Models;

// Garante que wwwroot exista ANTES do host ser criado. O ASP.NET Core decide
// se IWebHostEnvironment.WebRootFileProvider vira um PhysicalFileProvider (serve
// arquivos) ou um NullFileProvider (sempre 404) no momento em que o builder é
// montado, checando se a pasta wwwroot já existe naquele instante. O
// FileStorageService cria "wwwroot/uploads/..." sob demanda no primeiro upload,
// mas se wwwroot ainda não existisse quando a API subiu, o provider já havia
// sido travado como NullFileProvider — e nenhum arquivo enviado depois disso
// seria servido até a API ser reiniciada. Criando a pasta aqui, antes do
// CreateBuilder, isso não acontece mais.
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.AppContext.BaseDirectory, "wwwroot"));
System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot"));

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers e serialização JSON (enums como strings)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Configuração de CORS (Essencial para o Swagger e para o React/Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Documentação da API (Swagger / OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CloudShopping API",
        Version = "v1",
        Description = "API Multi-Tenant de E-commerce baseada em Clean Architecture e DDD."
    });

    // Define o Header X-Tenant-Id
    c.AddSecurityDefinition("X-Tenant-Id", new OpenApiSecurityScheme
    {
        Name = "X-Tenant-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Identificador do Tenant (multi-tenant). Ex: 1"
    });

    // Aplica a obrigatoriedade do Header no Swagger UI para todas as requisições
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-Tenant-Id"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 4. Camadas da aplicação (Application + Infrastructure)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");

// Serve os arquivos salvos em wwwroot/uploads/... (imagens de produto).
// Faltava esse middleware: o FileStorageService já gravava os arquivos em
// wwwroot, mas nada os expunha via HTTP, então o caminho relativo devolvido
// pelo upload (ex.: uploads/1/products/45/foto.jpg) resultava em 404.
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();