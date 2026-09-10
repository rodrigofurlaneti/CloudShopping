using CloudShopping.Domain.Exceptions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Behaviors;
public static class UseCaseExecution
{
    public static async Task<Result<T>> Run<T>(Func<Task<T>> execute, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try { return Result.Success(await execute()); }
        catch (ArgumentException ex) { return Result.Failure<T>(new Error("UseCase.Invalid", ex.Message)); }
        catch (KeyNotFoundException) { return Result.Failure<T>(new Error("UseCase.NotFound", "Registro não encontrado.")); }
        catch (UnauthorizedAccessException) { return Result.Failure<T>(new Error("UseCase.Forbidden", "Operação não autorizada.")); }
        catch (CommerceConflictException ex) { return Result.Failure<T>(new Error("UseCase.Conflict", ex.Message)); }
    }
}
