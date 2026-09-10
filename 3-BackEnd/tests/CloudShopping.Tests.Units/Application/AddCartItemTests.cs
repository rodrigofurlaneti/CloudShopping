using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Carts.Commands.AddCartItem;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Products;
using FluentAssertions;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;

public class AddCartItemTests
{
    [Theory]
    [InlineData(false,false,1,"Cart.NotFound")]
    [InlineData(true,false,1,"Product.NotFound")]
    [InlineData(true,true,0,"Cart.InvalidQuantity")]
    [InlineData(true,true,-1,"Cart.InvalidQuantity")]
    public async Task Invalid_request_never_persists(bool hasCart,bool hasProduct,int quantity,string code)
    {
        var carts=new Mock<ICartRepository>();var products=new Mock<IProductRepository>();
        var uow=new Mock<IUnitOfWork>(MockBehavior.Strict);
        carts.Setup(x=>x.GetByIdAsync(1,It.IsAny<CancellationToken>())).ReturnsAsync(hasCart?Cart.Create(1):null);
        products.Setup(x=>x.GetByIdAsync(2,It.IsAny<CancellationToken>())).ReturnsAsync(hasProduct?Product.Create(1,1,"SKU","Produto",10):null);
        var result=await new AddCartItemCommandHandler(carts.Object,products.Object,uow.Object).Handle(new(1,2,quantity),default);
        result.IsFailure.Should().BeTrue();result.Error.Code.Should().Be(code);
        carts.Verify(x=>x.Update(It.IsAny<Cart>()),Times.Never);
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Valid_request_updates_cart_and_commits_with_the_request_token()
    {
        using var cts=new CancellationTokenSource();var ct=cts.Token;
        var cart=Cart.Create(7);var product=Product.Create(1,1,"SKU","Produto",15);
        var carts=new Mock<ICartRepository>(MockBehavior.Strict);var products=new Mock<IProductRepository>(MockBehavior.Strict);
        var uow=new Mock<IUnitOfWork>(MockBehavior.Strict);
        carts.Setup(x=>x.GetByIdAsync(1,ct)).ReturnsAsync(cart);
        products.Setup(x=>x.GetByIdAsync(2,ct)).ReturnsAsync(product);
        carts.Setup(x=>x.Update(cart));uow.Setup(x=>x.CommitAsync(ct)).ReturnsAsync(1);
        var result=await new AddCartItemCommandHandler(carts.Object,products.Object,uow.Object).Handle(new(1,2,3),ct);
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().ContainSingle().Which.Quantity.Should().Be(3);
        cart.Items.Single().UnitPrice.Should().Be(15);
        carts.Verify(x=>x.Update(cart),Times.Once);uow.Verify(x=>x.CommitAsync(ct),Times.Once);
    }
}
