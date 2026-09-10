using CloudShopping.Domain.Entities.Carts;
using FluentAssertions;
using Xunit;
namespace CloudShopping.Tests.Units.Domain;
public class CartTests
{
    [Fact]
    public void Repeated_product_accumulates_quantity_then_can_be_replaced_removed_and_cleared()
    {
        var cart=Cart.Create(7);cart.CustomerId.Should().Be(7);
        cart.AddOrUpdateItem(1,2,10);cart.AddOrUpdateItem(1,3,10);
        cart.Items.Should().ContainSingle().Which.Quantity.Should().Be(5);
        cart.SetQuantity(1,2);cart.Items.Single().Quantity.Should().Be(2);
        cart.AddOrUpdateItem(2,1,20);cart.RemoveItem(1);
        cart.Items.Should().ContainSingle().Which.ProductId.Should().Be(2);
        cart.RemoveItem(999);cart.Items.Should().ContainSingle();
        cart.Clear();cart.Items.Should().BeEmpty();cart.ExpiresAt.Should().Be(cart.UpdatedAt.AddDays(30));
    }
    [Theory]
    [InlineData(0)] [InlineData(-1)]
    public void Invalid_quantity_preserves_existing_item(int quantity)
    {
        var cart=Cart.Create(1);cart.AddOrUpdateItem(1,2,10);
        var act=()=>cart.SetQuantity(1,quantity);act.Should().Throw<ArgumentException>();
        cart.Items.Single().Quantity.Should().Be(2);
    }
    [Fact]
    public void Cannot_change_missing_item()
    {var cart=Cart.Create(1);var act=()=>cart.SetQuantity(99,1);act.Should().Throw<KeyNotFoundException>();cart.Items.Should().BeEmpty();}
}
