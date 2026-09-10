using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Coupons.Commands.CreateCoupon;
using CloudShopping.Application.Features.Coupons.Commands.SetCouponEnabled;
using CloudShopping.Application.Features.Coupons.ViewModels;
using CloudShopping.Domain.Entities.Promotions;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using MediatR;
using Moq;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CouponUseCaseSteps
{
    private readonly Mock<ICouponRepository> repository=new(MockBehavior.Strict);private readonly Mock<IUnitOfWork> work=new(MockBehavior.Strict);private readonly Mock<ITenantProvider> tenant=new(MockBehavior.Strict);
    private string condition="";private Result<CouponView>? created;private Result<Unit>? updated;private Coupon coupon=null!;private Coupon? saved;
    [Given(@"um cadastro de cupom na condição ""(.*)""")]
    public void GivenCoupon(string status)
    {
        condition=status;tenant.Setup(x=>x.GetTenantId()).Returns(7);work.Setup(x=>x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        repository.Setup(x=>x.CodeExistsAsync("PROMO",It.IsAny<CancellationToken>())).ReturnsAsync(status=="duplicado");
        repository.Setup(x=>x.AddAsync(It.IsAny<Coupon>(),It.IsAny<CancellationToken>())).Callback<Coupon,CancellationToken>((c,_)=>saved=c).Returns(Task.CompletedTask);
        coupon=Coupon.Create(7,"PROMO","Fixed",10,0,2,1,DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddDays(1));
        repository.Setup(x=>x.GetByIdAsync(coupon.Id,It.IsAny<CancellationToken>())).ReturnsAsync(status=="ausente"?null:coupon);
    }
    [When("o caso de uso cadastra o cupom na loja atual")]
    public async Task Create()=>created=await new CreateCouponCommandHandler(repository.Object,work.Object,tenant.Object).Handle(new(" promo ","Fixed",condition=="valor inválido"?-1:10,0,2,1,DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddDays(1)),default);
    [When("o caso de uso altera a ativação do cupom")]
    public async Task Enable()=>updated=await new SetCouponEnabledCommandHandler(repository.Object,work.Object).Handle(new(coupon.Id,condition=="concorrente"?2:1,condition=="sem alteração"),default);
    [Then(@"a operação de cupom retorna ""(.*)"" e grava (.*) vezes")]
    public void Verify(string code,int writes)
    {
        Result result=(Result?)created??updated!;
        if(code=="sucesso")result.IsSuccess.Should().BeTrue();else result.Error.Code.Should().Be(code);
        work.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Exactly(writes));
        if(created!=null){repository.Verify(x=>x.AddAsync(It.IsAny<Coupon>(),It.IsAny<CancellationToken>()),Times.Exactly(writes));if(saved!=null){saved.TenantId.Should().Be(7);saved.Code.Should().Be("PROMO");}}
        else coupon.Enabled.Should().Be(writes==0);
    }
}
