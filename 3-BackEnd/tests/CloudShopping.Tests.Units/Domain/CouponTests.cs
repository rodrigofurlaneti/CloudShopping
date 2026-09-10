using CloudShopping.Domain.Entities.Promotions;
using FluentAssertions;
using Xunit;

namespace CloudShopping.Tests.Units.Domain;
public class CouponTests
{
    [Theory]
    [InlineData(null,null)] [InlineData(" ",null)] [InlineData(" promo-10 ","PROMO-10")]
    public void Codes_are_normalized(string? input,string? expected)=>Coupon.Normalize(input).Should().Be(expected);
    [Theory]
    [InlineData("ab")] [InlineData("PROMO 10")] [InlineData("PROMO!")]
    public void Invalid_code_is_rejected(string input)
    {var act=()=>Coupon.Normalize(input);act.Should().Throw<ArgumentException>();}

    [Theory]
    [InlineData("Percent",101,0,10,1)] [InlineData("Other",10,0,10,1)]
    [InlineData("Fixed",0,0,10,1)] [InlineData("Fixed",10,-1,10,1)]
    [InlineData("Fixed",10,0,0,1)] [InlineData("Fixed",10,0,10,11)]
    [InlineData("Fixed",10.001,0,10,1)]
    public void Invalid_rules_prevent_creation(string kind,decimal value,decimal minimum,int limit,int customerLimit)
    {
        var now=DateTime.UtcNow;
        var act=()=>Coupon.Create(1,"PROMO",kind,value,minimum,limit,customerLimit,now,now.AddDays(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Usage_limits_cannot_be_exceeded_or_released_twice()
    {
        var now=DateTime.UtcNow;var coupon=Coupon.Create(1," promo ","Fixed",10,0,1,1,now,now.AddDays(1));
        coupon.Code.Should().Be("PROMO");coupon.Enabled.Should().BeTrue();
        coupon.Redeem();coupon.UsedCount.Should().Be(1);
        var redeem=()=>coupon.Redeem();redeem.Should().Throw<InvalidOperationException>();
        coupon.UsedCount.Should().Be(1);coupon.Release();coupon.UsedCount.Should().Be(0);
        var release=()=>coupon.Release();release.Should().Throw<InvalidOperationException>();
        coupon.SetEnabled(false);coupon.Enabled.Should().BeFalse();
    }
}
