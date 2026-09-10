using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.LogTrackers;
using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Primitives.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;

public class ProcessingLogBehaviorTests
{
    private sealed record SampleCommand;
    private sealed record Context(bool RequestAborted = false) : IProcessingLogContext
    {
        public int? TenantId => 1;
        public long? ActorId => 2;
        public string? ActorType => "Administrator";
        public string? TraceId => "controller-trace";
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Preserves_result_and_records_outcome(bool fails)
    {
        LogTracker? saved = null;
        var writer = new Mock<IProcessingLogWriter>();
        writer.Setup(x => x.WriteAsync(It.IsAny<LogTracker>(), It.IsAny<CancellationToken>()))
            .Callback<LogTracker, CancellationToken>((entry, _) => saved = entry).Returns(Task.CompletedTask);
        var behavior = new ProcessingLogBehavior<SampleCommand, Result<int>>(
            new ProcessingLogRecorder(writer.Object, NullLogger<ProcessingLogRecorder>.Instance), new Context());
        var result = fails ? Result.Failure<int>(new Error("invalid", "private data")) : Result.Success(42);
        var returned = await behavior.Handle(new(), _ => Task.FromResult(result), default);
        Assert.Same(result, returned);
        Assert.NotNull(saved);
        Assert.Equal(fails ? "Error" : "Success", saved.Outcome);
        Assert.Equal("controller-trace", saved.TraceId);
        Assert.Equal("Application", saved.DirectoryName);
        Assert.Equal("SampleCommand", saved.ClassName);
        Assert.True(saved.ExecutionTimeMs >= 0);
        Assert.Null(saved.HttpStatusCode);
        Assert.DoesNotContain("private data", saved.Message ?? "");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Preserves_exception_and_logs_using_independent_token(bool cancel)
    {
        LogTracker? saved = null;
        var writer = new Mock<IProcessingLogWriter>();
        writer.Setup(x => x.WriteAsync(It.IsAny<LogTracker>(), It.IsAny<CancellationToken>()))
            .Callback<LogTracker, CancellationToken>((entry, token) => { saved = entry; Assert.False(token.IsCancellationRequested); })
            .Returns(Task.CompletedTask);
        var behavior = new ProcessingLogBehavior<SampleCommand, Result>(
            new ProcessingLogRecorder(writer.Object, NullLogger<ProcessingLogRecorder>.Instance), new Context(cancel));
        Exception failure = cancel ? new OperationCanceledException("private") : new InvalidOperationException("private");
        var thrown = await Record.ExceptionAsync(() => behavior.Handle(new(), _ => Task.FromException<Result>(failure), new CancellationToken(cancel)));
        Assert.Same(failure, thrown);
        Assert.NotNull(saved);
        Assert.Equal(cancel ? "Cancelled" : "Error", saved.Outcome);
        Assert.Equal(cancel, saved.RequestAborted);
        Assert.Equal(failure.GetType().FullName, saved.ExceptionType);
        Assert.DoesNotContain("private", saved.ErrorMessage ?? "");
    }
}

