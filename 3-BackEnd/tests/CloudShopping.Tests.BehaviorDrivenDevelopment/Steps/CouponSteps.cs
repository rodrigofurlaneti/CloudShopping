using CloudShopping.Domain.Entities.Promotions;
using FluentAssertions;
using Reqnroll;
namespace CloudShopping.Tests.Specs.Steps;
[Binding]
public sealed class CouponSteps
{
    private string kind="Fixed",code="PROMO";private decimal value=10,minimum;private int limit=2,perCustomer=1;private DateTime start=DateTime.UtcNow.AddHours(-1),end=DateTime.UtcNow.AddDays(1);private Coupon? coupon;private Exception? error;
    [Given(@"um cupom do tipo ""(.*)"" com valor (.*), mínimo (.*), limite (.*) e limite por cliente (.*)")]
    public void GivenCoupon(string type,decimal amount,decimal subtotal,int totalLimit,int customerLimit){kind=type;value=amount;minimum=subtotal;limit=totalLimit;perCustomer=customerLimit;}
    [Given(@"o código do cupom é ""(.*)""")]
    public void Code(string input)=>code=input;
    [Given("a vigência do cupom já terminou")]
    public void Expired(){start=DateTime.UtcNow.AddDays(-2);end=DateTime.UtcNow.AddDays(-1);}
    [When("o cupom é criado")]
    public void Create(){try{coupon=Coupon.Create(1,code,kind,value,minimum,limit,perCustomer,start,end);}catch(Exception e){error=e;}}
    [Then(@"o cadastro do cupom é ""(.*)""")]
    public void Verify(string result){if(result=="aceito"){error.Should().BeNull();coupon.Should().NotBeNull();coupon!.Code.Should().Be(code.Trim().ToUpperInvariant());}else error.Should().BeOfType<ArgumentException>();}
    [When(@"o cupom é utilizado (.*) vezes e liberado (.*) vezes")]
    public void Consume(int use,int release){coupon=Coupon.Create(1,"PROMO",kind,value,minimum,limit,perCustomer,start,end);try{for(var i=0;i<use;i++)coupon.Redeem();for(var i=0;i<release;i++)coupon.Release();}catch(Exception e){error=e;}}
    [Then(@"o contador do cupom é (.*) e a utilização foi ""(.*)""")]
    public void Counter(int count,string result){coupon!.UsedCount.Should().Be(count);if(result=="aceita")error.Should().BeNull();else error.Should().BeOfType<InvalidOperationException>();}
}
