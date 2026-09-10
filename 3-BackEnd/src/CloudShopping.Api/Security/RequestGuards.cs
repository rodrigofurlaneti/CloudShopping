using CloudShopping.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using CloudShopping.Infrastructure.Services;
namespace CloudShopping.Api.Security;
public sealed class RequestGuards(AppDbContext db) : IAsyncActionFilter, IOrderedFilter
{
    public int Order => -3000;
    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        if (ctx.HttpContext.User.IsInRole("Administrator") && ctx.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() == null)
        {
            var descriptor = (ControllerActionDescriptor)ctx.ActionDescriptor;
            var permissions = await new StorePermissions(db).ForUser(StoreSecurity.Subject(ctx.HttpContext.User), ctx.HttpContext.RequestAborted);
            var required = AccessRequirements.For(descriptor.ControllerName, descriptor.ActionName, HttpMethods.IsGet(ctx.HttpContext.Request.Method) || HttpMethods.IsHead(ctx.HttpContext.Request.Method));
            if (required.Any(x => !StorePermissions.Allows(permissions, x)))
            { ctx.Result = new ObjectResult(new { message = "Seu perfil não possui permissão para esta operação." }) { StatusCode = 403 }; return; }
        }
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
        if ((path.StartsWith("/api/v1/orders", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/api/v1/order-state-histories", StringComparison.OrdinalIgnoreCase)) &&
            ctx.HttpContext.Request.Method != "GET")
        {
            ctx.Result = new ConflictObjectResult(new { message = "Operação legada desabilitada. Use o checkout, a operação de pedidos e o financeiro. O histórico existente não pode ser apagado ou reescrito." });
            return;
        }
        await next();
    }
}
