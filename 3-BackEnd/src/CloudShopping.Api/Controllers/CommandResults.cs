using CloudShopping.Domain.Primitives.Results;
using Microsoft.AspNetCore.Mvc;
namespace CloudShopping.Api.Controllers;
internal static class CommandResults
{
    public static IActionResult Respond<T>(Result<T> result, Func<T, IActionResult> success)
    {
        if (result.IsSuccess) return success(result.Value);
        var status = result.Error.Code.EndsWith(".NotFound") ? 404 : result.Error.Code.EndsWith(".Forbidden") ? 403 : result.Error.Code.EndsWith(".Conflict") ? 409 : 400;
        return new ObjectResult(new { message = result.Error.Message }) { StatusCode = status };
    }
}
