using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Notifications.Commands.MarkNotificationRead;
using CloudShopping.Domain.Entities.Notifications;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using MediatR;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class NotificationSteps
{
    private readonly Mock<INotificationRepository> repository=new(MockBehavior.Strict);private readonly Mock<IUnitOfWork> work=new(MockBehavior.Strict);private CustomerNotification notification=null!;private DateTime? firstRead;private Result<Unit> result=null!;
    [Given(@"uma notificação na condição ""(.*)""")]
    public void GivenNotification(string condition)
    {
        notification=CustomerNotification.Create(1,10,20,"evento","OrderCreated");if(condition=="já lida"){notification.MarkRead();firstRead=notification.ReadAt;}
        repository.Setup(x=>x.GetCustomerByIdAsync(10,notification.Id,It.IsAny<CancellationToken>())).ReturnsAsync(condition=="outro cliente"?null:notification);
        work.Setup(x=>x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }
    [When("o cliente marca a notificação como lida")]
    public async Task Read()=>result=await new MarkNotificationReadCommandHandler(repository.Object,work.Object).Handle(new(10,notification.Id),default);
    [Then(@"a leitura da notificação retorna ""(.*)"" e grava (.*) vezes")]
    public void Verify(string code,int count)
    {if(code=="sucesso"){result.IsSuccess.Should().BeTrue();notification.ReadAt.Should().NotBeNull();if(firstRead.HasValue)notification.ReadAt.Should().Be(firstRead);}else {result.Error.Code.Should().Be(code);notification.ReadAt.Should().BeNull();}work.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Exactly(count));}
}
