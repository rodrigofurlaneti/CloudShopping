using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.LogTrackers.Commands.CreateLogTracker;
using CloudShopping.Application.Features.LogTrackers.Commands.UpdateLogTracker;
using CloudShopping.Application.Features.LogTrackers.Commands.DeleteLogTracker;
using CloudShopping.Domain.Entities.Diagnostics;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class LogTrackerTests
{
    private static LogTrackerData Data => new(null,"System","trace",null,"Processor","Execute","Error",false,10,"GET",500,"InvalidOperationException","Falha de teste",null,null,null);
    [Theory]
    [InlineData("Success",true)] [InlineData("Error",false)] [InlineData("Cancelled",false)]
    public void Result_is_consistent_with_outcome(string outcome,bool success)
    {var log=LogTracker.Create(1,Data with {Outcome=outcome});Assert.Equal(success,log.IsSuccess);Assert.True(log.IsActive);Assert.Equal(1,log.TenantId);Assert.Equal(DateTimeKind.Utc,log.CreatedAt.Kind);}
    [Theory]
    [InlineData("name")] [InlineData("outcome")] [InlineData("duration")] [InlineData("status")] [InlineData("aborted")] [InlineData("actor")] [InlineData("text")]
    public void Invalid_data_cannot_change_existing_entry(string invalid)
    {
        var data=invalid switch {"name"=>Data with {ClassName=" "},"outcome"=>Data with {Outcome="Unknown"},"duration"=>Data with {ExecutionTimeMs=-1},"status"=>Data with {HttpStatusCode=99},"aborted"=>Data with {RequestAborted=true},"actor"=>Data with {AppUserId=1},_=>Data with {Message=new string('x',65536)}};
        var log=LogTracker.Create(1,Data);
        Assert.Throws<ArgumentException>(()=>log.UpdateDetails(data,false));
        Assert.True(log.IsActive);Assert.Null(log.UpdatedAt);Assert.Equal("Processor",log.ClassName);
    }
    [Fact]
    public async Task Create_uses_current_tenant_and_commits()
    {
        var repo=new Mock<ILogTrackerRepository>();var tenant=new Mock<ITenantProvider>();var uow=new Mock<IUnitOfWork>();
        tenant.Setup(x=>x.GetTenantId()).Returns(7);LogTracker? saved=null;
        repo.Setup(x=>x.AddAsync(It.IsAny<LogTracker>(),It.IsAny<CancellationToken>())).Callback<LogTracker,CancellationToken>((x,_)=>saved=x).Returns(Task.CompletedTask);
        var result=await new CreateLogTrackerCommandHandler(repo.Object,uow.Object,tenant.Object).Handle(new(Data),default);
        Assert.True(result.IsSuccess);Assert.Equal(7,saved!.TenantId);uow.Verify(x=>x.CommitAsync(default),Times.Once);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Update_rejects_missing_or_changed_record_without_commit(bool exists)
    {
        var repo=new Mock<ILogTrackerRepository>();var uow=new Mock<IUnitOfWork>(MockBehavior.Strict);
        var entry=LogTracker.Create(1,Data);entry.UpdateDetails(Data,true);
        repo.Setup(x=>x.GetByIdAsync(1,default)).ReturnsAsync(exists?entry:null);
        var result=await new UpdateLogTrackerCommandHandler(repo.Object,uow.Object).Handle(new(1,Data,false,null),default);
        Assert.Equal(exists?"Command.Conflict":"Command.NotFound",result.Error.Code);uow.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Update_preserves_creation_and_delete_removes_loaded_record()
    {
        var entry=LogTracker.Create(1,Data);var created=entry.CreatedAt;
        var repo=new Mock<ILogTrackerRepository>();var uow=new Mock<IUnitOfWork>();
        repo.Setup(x=>x.GetByIdAsync(1,default)).ReturnsAsync(entry);
        var update=await new UpdateLogTrackerCommandHandler(repo.Object,uow.Object).Handle(new(1,Data with {Outcome="Success"},false,null),default);
        Assert.True(update.IsSuccess);Assert.True(entry.IsSuccess);Assert.False(entry.IsActive);Assert.Equal(created,entry.CreatedAt);
        var deleted=await new DeleteLogTrackerCommandHandler(repo.Object,uow.Object).Handle(new(1,entry.UpdatedAt),default);
        Assert.True(deleted.IsSuccess);repo.Verify(x=>x.Remove(entry),Times.Once);uow.Verify(x=>x.CommitAsync(default),Times.Exactly(2));
    }
}
