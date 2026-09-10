using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class CommandExecutionTests
{
    [Fact]
    public async Task Success_preserves_returned_value() =>
        (await CommandExecution.Run(()=>Task.FromResult(42))).Value.Should().Be(42);

    [Theory]
    [InlineData("argument","Command.Invalid")]
    [InlineData("missing","Command.NotFound")]
    [InlineData("access","Command.Forbidden")]
    [InlineData("conflict","Command.Conflict")]
    [InlineData("state","Command.Conflict")]
    public async Task Expected_failures_are_converted_to_result(string kind,string code)
    {
        Exception error=kind switch {
            "argument"=>new ArgumentException("invalid"),
            "missing"=>new KeyNotFoundException("missing"),
            "access"=>new UnauthorizedAccessException("private"),
            "conflict"=>new CommerceConflictException("conflict"),
            _=>new InvalidOperationException("state")};
        var result=await CommandExecution.Run(()=>Task.FromException<int>(error));
        result.IsFailure.Should().BeTrue();result.Error.Code.Should().Be(code);
        if(kind=="access")result.Error.Message.Should().NotContain("private");
        else result.Error.Message.Should().Be(error.Message);
    }

    [Fact]
    public async Task Cancellation_is_propagated()
    {
        using var cts=new CancellationTokenSource();cts.Cancel();
        Func<Task> act=()=>CommandExecution.Run(()=>Task.FromCanceled<int>(cts.Token));
        (await act.Should().ThrowAsync<OperationCanceledException>()).Which.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task Unexpected_failure_is_not_disguised_as_business_rejection()
    {
        var failure=new IOException("storage failed");
        Func<Task> act=()=>CommandExecution.Run(()=>Task.FromException<int>(failure));
        (await act.Should().ThrowAsync<IOException>()).Which.Should().BeSameAs(failure);
    }
}
