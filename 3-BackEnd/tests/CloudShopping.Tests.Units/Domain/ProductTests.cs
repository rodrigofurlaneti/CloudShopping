using CloudShopping.Domain.Entities.Products;
using FluentAssertions;
using Xunit;

namespace CloudShopping.Tests.Units.Domain;

public class ProductTests
{
    [Theory]
    [InlineData(0,"SKU","Produto",10,0)]
    [InlineData(1,"","Produto",10,0)]
    [InlineData(1,"SKU","",10,0)]
    [InlineData(1,"SKU","Produto",0,0)]
    [InlineData(1,"SKU","Produto",10,-1)]
    public void Creation_rejects_invalid_catalog_data(int department,string sku,string name,decimal price,int stock)
    {
        var act=()=>Product.Create(1,department,sku,name,price,stock);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reservation_commit_and_release_preserve_available_inventory()
    {
        var p=Product.Create(1,1,"SKU","Produto",10,20);
        p.ReserveStock(8);
        p.AvailableStock.Should().Be(12);
        p.CommitReservedStock(3);
        p.PhysicalStock.Should().Be(17);
        p.ReservedStock.Should().Be(5);
        p.ReleaseReservedStock(5);
        p.AvailableStock.Should().Be(17);
    }

    [Theory]
    [InlineData("reserve",0)] [InlineData("reserve",21)]
    [InlineData("release",6)] [InlineData("commit",6)]
    [InlineData("adjust",4)] [InlineData("adjust",-1)]
    [InlineData("add",0)] [InlineData("add",-1)]
    public void Invalid_inventory_operation_does_not_change_stock(string operation,int quantity)
    {
        var p=Product.Create(1,1,"SKU","Produto",10,20);p.ReserveStock(5);
        Action act=()=> { switch(operation) {
            case "reserve":p.ReserveStock(quantity);break;
            case "release":p.ReleaseReservedStock(quantity);break;
            case "commit":p.CommitReservedStock(quantity);break;
            case "adjust":p.AdjustInventory(quantity);break;
            case "add":p.AddPhysicalStock(quantity);break;
        }};
        act.Should().Throw<Exception>();
        p.PhysicalStock.Should().Be(20);p.ReservedStock.Should().Be(5);
    }

    [Fact]
    public void Catalog_updates_preserve_identity_and_change_sellable_details()
    {
        var p=Product.Create(1,1,"SKU","Original",10,20);
        p.UpdateDetails("Novo",25);p.ChangeDepartment(2);
        p.UpdateLocation(StockLocation.Create(" a ","b","c","d"));
        p.Location!.ToString().Should().Be("A-B-C-D");
        p.ClearLocation();p.Location.Should().BeNull();
        p.AddPhysicalStock(2);p.AdjustInventory(30);
        p.Name.Should().Be("Novo");p.Price.Should().Be(25);
        p.DepartmentId.Should().Be(2);p.Sku.Should().Be("SKU");
        p.PhysicalStock.Should().Be(30);
    }
}
