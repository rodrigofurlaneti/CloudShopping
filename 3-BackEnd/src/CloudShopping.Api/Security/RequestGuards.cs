using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Api.Security;
public sealed class RequestGuards(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        foreach (var value in ctx.ActionArguments.Values)
        {
            var prop = value?.GetType().GetProperty("TenantId");
            if (prop?.GetValue(value) is int requested && requested > 0 && requested != db.CurrentTenantId)
            { ctx.Result = new ForbidResult(); return; }
        }
        foreach (var name in new[] { "page", "pageSize" })
            if (ctx.HttpContext.Request.Query.TryGetValue(name, out var v) &&
                (!int.TryParse(v, out var n) || n <= 0 || (name == "pageSize" && n > 200)))
            { ctx.Result = new BadRequestObjectResult(new { message = "Paginação inválida (page >= 1; pageSize entre 1 e 200)." }); return; }
        var path = ctx.HttpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/api/v1/orders", StringComparison.OrdinalIgnoreCase) &&
            ctx.HttpContext.Request.Method != "GET")
        {
            ctx.Result = new ConflictObjectResult(new { message = "Operação legada desabilitada. Use o checkout da loja; pagamentos e expedição serão integrados na próxima etapa." });
            return;
        }
        await next();
    }
}

