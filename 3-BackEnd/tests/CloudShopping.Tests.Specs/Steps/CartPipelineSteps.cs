using CloudShopping.Application;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Commands.ChangeStoreCart;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CartPipelineSteps
{
    private readonly Mock<IStoreCommerce> commerce=new(MockBehavior.Strict);private Result<CartView> result=null!;
    [When(@"o pipeline altera o carrinho do cliente (.*), produto (.*), operação ""(.*)"" e quantidade (.*)")]
    public async Task Execute(int customer,int product,string operation,int quantity)
    {
        commerce.Setup(x=>x.ChangeCart(customer,product,quantity,operation,It.IsAny<CancellationToken>())).ReturnsAsync(new CartView(1,1,DateTime.UtcNow.AddDays(1),[],0));
        var services=new ServiceCollection();services.AddLogging();services.AddApplication();services.AddSingleton(commerce.Object);
        await using var provider=services.BuildServiceProvider();await using var scope=provider.CreateAsyncScope();
        result=await scope.ServiceProvider.GetRequiredService<ISender>().Send(new ChangeStoreCartCommand(customer,product,quantity,operation));
    }
    [Then(@"a alteração pelo pipeline é aceita (.*) e chama o serviço (.*) vezes")]
    public void Verify(bool accepted,int calls){result.IsSuccess.Should().Be(accepted);commerce.Verify(x=>x.ChangeCart(It.IsAny<int>(),It.IsAny<int>(),It.IsAny<int>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Exactly(calls));}
}
