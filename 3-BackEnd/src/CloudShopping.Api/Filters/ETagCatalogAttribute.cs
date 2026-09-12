using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CloudShopping.Api.Filters
{
    // Estratégia HTTP Cache (ETag / Cache-Control) exigida pela tarefa de cache Redis
    // para as listagens estáticas de products, productimages (embutidas no produto),
    // departments e storebanners: calcula um ETag forte a partir do próprio corpo da
    // resposta (equivalente, na prática, a um ETag por UpdatedAt/Version — o hash muda
    // exatamente quando o conteúdo muda) e responde 304 Not Modified quando o cliente já
    // possui essa versão em cache, poupando banda como pedido no DoD.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ETagCatalogAttribute : Attribute, IAsyncActionFilter
    {
        public int MaxAgeSeconds { get; init; } = 120;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();
            if (executed.Exception != null && !executed.ExceptionHandled) return;

            if (executed.Result is ObjectResult { Value: not null } objectResult &&
                (objectResult.StatusCode is null || objectResult.StatusCode == StatusCodes.Status200OK))
            {
                var json = JsonSerializer.SerializeToUtf8Bytes(objectResult.Value);
                var hash = Convert.ToHexString(SHA256.HashData(json));
                var etag = $"\"{hash}\"";

                var response = context.HttpContext.Response;
                response.Headers.CacheControl = $"private, max-age={MaxAgeSeconds}, must-revalidate";
                response.Headers.ETag = etag;

                var ifNoneMatch = context.HttpContext.Request.Headers.IfNoneMatch.ToString();
                if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag)
                {
                    executed.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                }
            }
        }
    }
}
