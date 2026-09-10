using CloudShopping.Domain.Exceptions;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Behaviors;

// Common conversion of expected use-case failures to the project's Result contract.
public static class CommandExecution
{
    public static async Task<Result<T>> Run<T>(Func<Task<T>> execute)
    {
        try { return Result.Success(await execute()); }
        catch (ArgumentException e) { return Result.Failure<T>(new Error("Command.Invalid", e.Message)); }
        catch (KeyNotFoundException e) { return Result.Failure<T>(new Error("Command.NotFound", e.Message)); }
        catch (UnauthorizedAccessException) { return Result.Failure<T>(new Error("Command.Forbidden", "Operação não autorizada.")); }
        catch (CommerceConflictException e) { return Result.Failure<T>(new Error("Command.Conflict", e.Message)); }
        catch (InvalidOperationException e) { return Result.Failure<T>(new Error("Command.Conflict", e.Message)); }
    }
}
