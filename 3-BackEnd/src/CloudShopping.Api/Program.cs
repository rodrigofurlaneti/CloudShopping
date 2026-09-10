using CloudShopping.Domain.Exceptions;
using Microsoft.AspNetCore.DataProtection;
using CloudShopping.Application;
using CloudShopping.Infrastructure;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using CloudShopping.Api.Security;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));
builder.Services.AddControllers(o => o.Filters.Add<RequestGuards>(-3000))
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddScoped<RequestGuards>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<ReservationExpiryWorker>();
builder.Services.AddScoped<CloudShopping.Infrastructure.Operations.OrderOperations>();
builder.Services.AddScoped<CloudShopping.Infrastructure.Operations.CustomerEngagement>();

builder.Services.AddHostedService<CloudShopping.Infrastructure.Operations.NotificationWorker>();
builder.Services.AddHostedService<CloudShopping.Infrastructure.Operations.ImportWorker>();
builder.Services.AddSingleton<AsaasInbox>();
builder.Services.AddHostedService<AsaasWorker>();
builder.Services.AddProblemDetails();
var keyDirectory = Path.GetFullPath(builder.Configuration["DataProtection:KeyPath"] ?? Path.Combine(builder.Environment.ContentRootPath, ".local", "keys"));
Directory.CreateDirectory(keyDirectory);
builder.Services.AddDataProtection().SetApplicationName("CloudShopping").PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
builder.Services.AddAntiforgery(o => { o.HeaderName = "X-CSRF-Token"; o.Cookie.SameSite = SameSiteMode.Strict; });
builder.Services.AddAuthentication(StoreSecurity.Scheme).AddCookie(StoreSecurity.Scheme, o =>
{
    o.Cookie.Name = "cloudshopping.session";
    o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    o.SlidingExpiration = false; o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.Events.OnValidatePrincipal = StoreSecurity.Validate;
    o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder(StoreSecurity.Scheme)
    .RequireAuthenticatedUser().RequireRole("Administrator").Build());
builder.Services.AddRateLimiter(o => {
    o.RejectionStatusCode = 429;
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown") +
            (ctx.Request.Path.StartsWithSegments("/api/v1/session") && ctx.Request.Method == "POST" ? ":auth" : ":api"),
            key => new FixedWindowRateLimiterOptions { PermitLimit = key.EndsWith(":auth") ? 20 : 300,
                Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173" })
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseExceptionHandler(error => error.Run(async ctx => {
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var status = ex switch {
        UnauthorizedAccessException => 403, KeyNotFoundException => 404,
        ValidationException or ArgumentException or System.Text.Json.JsonException => 400,
        AsaasApiException => 502,
        DbUpdateConcurrencyException or CommerceConflictException => 409,
        InvalidOperationException => 409, _ => 500
    };
    var message = status == 500 ? "Não foi possível concluir. Informe o identificador da solicitação ao suporte." : ex?.Message;
    ctx.Response.StatusCode = status;
    await Results.Problem(statusCode: status, title: message,
        extensions: new Dictionary<string,object?> { ["message"] = message, ["traceId"] = ctx.TraceIdentifier }).ExecuteAsync(ctx);
}));
// Product and banner images are public; administrative upload endpoints remain authorized.
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.Use(StoreSecurity.ResolveTenant);
app.UseAuthorization();
app.Use(async (ctx, next) => {
    if (ctx.GetEndpoint()?.Metadata.GetMetadata<AsaasWebhookAttribute>() == null && ctx.Request.Path.StartsWithSegments("/api") && !HttpMethods.IsGet(ctx.Request.Method) &&
        !HttpMethods.IsHead(ctx.Request.Method) && !HttpMethods.IsOptions(ctx.Request.Method))
    {
        try { await ctx.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(ctx); }
        catch (AntiforgeryValidationException) {
            ctx.Response.StatusCode = 400;
            await ctx.Response.WriteAsJsonAsync(new { message = "Sessão de formulário expirada. Atualize a página." }); return;
        }
    }
    await next();
});
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();
app.Run();

public partial class Program { }
