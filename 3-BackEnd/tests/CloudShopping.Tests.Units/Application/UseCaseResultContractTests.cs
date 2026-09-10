using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Xunit;
namespace CloudShopping.Tests.Units.Application;
public class UseCaseResultContractTests
{
    [Fact]
    public void Every_mediatr_request_uses_result()
    {
        var violations = typeof(UseCaseExecution).Assembly.GetTypes()
            .SelectMany(t => t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .Where(i => !typeof(Result).IsAssignableFrom(i.GetGenericArguments()[0])).Select(_ => t.FullName));
        Assert.Empty(violations);
        Assert.Empty(typeof(UseCaseExecution).Assembly.GetTypes().Where(t => typeof(IRequest).IsAssignableFrom(t)));
    }
    [Fact]
    public async Task Expected_failure_is_a_result()
    {
        var result = await UseCaseExecution.Run<int>(() => Task.FromException<int>(new ArgumentException("Inválido")), default);
        Assert.True(result.IsFailure);
        Assert.Equal("UseCase.Invalid", result.Error.Code);
    }
    [Fact]
    public async Task Technical_failure_is_not_business_conflict()
    {
        var failure = new InvalidOperationException("Provider failed");
        var actual = await Record.ExceptionAsync(() => UseCaseExecution.Run<int>(() => Task.FromException<int>(failure), default));
        Assert.Same(failure, actual);
    }
    [Fact]
    public async Task Cancellation_is_preserved()
    {
        var failure = new OperationCanceledException();
        var actual = await Record.ExceptionAsync(() => UseCaseExecution.Run<int>(() => Task.FromException<int>(failure), default));
        Assert.Same(failure, actual);
    }
}
