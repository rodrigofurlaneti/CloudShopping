using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.LogTrackers;
using CloudShopping.Domain.Entities.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
namespace CloudShopping.Tests.Units.Application;
public class ProcessingLogRecorderTests
{
    private static LogTrackerData Data => new(null,null,"trace", "Controllers", "TenantsController", "Create", "Success",false,20,"POST",201,null,"Processamento concluído.",null,null,null);
    [Fact]
    public async Task Recorder_writes_without_requiring_an_authenticated_tenant()
    {
        var writer=new Mock<IProcessingLogWriter>(MockBehavior.Strict);
        writer.Setup(x=>x.WriteAsync(It.Is<LogTracker>(l=>l.TenantId==null && l.IsSuccess && l.TraceId=="trace"),It.Is<CancellationToken>(ct=>ct.CanBeCanceled && !ct.IsCancellationRequested))).Returns(Task.CompletedTask);
        await new ProcessingLogRecorder(writer.Object,NullLogger<ProcessingLogRecorder>.Instance).RecordAsync(null,Data);
        writer.VerifyAll();
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Persistence_failure_does_not_escape_recorder(bool cancellation)
    {
        var writer=new Mock<IProcessingLogWriter>();
        writer.Setup(x=>x.WriteAsync(It.IsAny<LogTracker>(),It.IsAny<CancellationToken>())).ThrowsAsync(cancellation?new OperationCanceledException():new InvalidOperationException("Database unavailable"));
        var error=await Record.ExceptionAsync(()=>new ProcessingLogRecorder(writer.Object,NullLogger<ProcessingLogRecorder>.Instance).RecordAsync(1,Data));
        Assert.Null(error);writer.Verify(x=>x.WriteAsync(It.IsAny<LogTracker>(),It.IsAny<CancellationToken>()),Times.Once);
    }
}
