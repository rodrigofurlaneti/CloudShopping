using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Carts.Commands.AddCartItem;
using CloudShopping.Domain.Primitives.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class ValidationBehaviorTests
{
    public sealed record GenericRequest(int Value):IRequest<Result<int>>;
    public sealed record PlainRequest(int Value):IRequest<int>;
    [Theory]
    [InlineData(0,1,1)] [InlineData(1,0,1)] [InlineData(1,1,0)] [InlineData(1,1,-1)]
    public async Task Invalid_cart_request_never_invokes_next(int cart,int product,int quantity)
    {
        var behavior=new ValidationBehavior<AddCartItemCommand,Result>([new AddCartItemCommandValidator()]);
        var invoked=false;
        var result=await behavior.Handle(new(cart,product,quantity),_=>{invoked=true;return Task.FromResult(Result.Success());},default);
        result.IsFailure.Should().BeTrue();invoked.Should().BeFalse();
    }
    [Theory]
    [InlineData(true)] [InlineData(false)]
    public async Task Valid_request_invokes_next_once_with_or_without_validators(bool validate)
    {
        var behavior=new ValidationBehavior<AddCartItemCommand,Result>(validate?[new AddCartItemCommandValidator()]:[]);
        var count=0;var expected=Result.Success();
        var actual=await behavior.Handle(new(1,1,1),_=>{count++;return Task.FromResult(expected);},default);
        actual.Should().BeSameAs(expected);count.Should().Be(1);
    }
    [Fact]
    public async Task Generic_result_failure_keeps_correct_runtime_type()
    {
        var validator=new InlineValidator<GenericRequest>();validator.RuleFor(x=>x.Value).GreaterThan(0);
        var result=await new ValidationBehavior<GenericRequest,Result<int>>([validator]).Handle(new(0),_=>throw new Xunit.Sdk.XunitException("Handler must not run"),default);
        result.IsFailure.Should().BeTrue();result.Error.Code.Should().Be("Value");
    }
    [Fact]
    public async Task Non_result_response_throws_validation_exception()
    {
        var validator=new InlineValidator<PlainRequest>();validator.RuleFor(x=>x.Value).GreaterThan(0);
        Func<Task> act=()=>new ValidationBehavior<PlainRequest,int>([validator]).Handle(new(0),_=>throw new Xunit.Sdk.XunitException("Handler must not run"),default);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
