using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.Commands.AdjustInventory;
using CloudShopping.Application.Features.Products.Commands.AddProductStock;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class InventoryTests
{
    private readonly Mock<IProductRepository> products=new();
    private readonly Mock<IStockMovementRepository> movements=new();
    private readonly Mock<IUnitOfWork> uow=new();
    private readonly Mock<ITenantProvider> tenant=new();
    public InventoryTests(){tenant.Setup(x=>x.GetTenantId()).Returns(1);}
    private Product Existing(int owner=1)
    {
        var p=Product.Create(owner,1,"SKU","Produto",10,20);
        // Simulates the identity assigned by persistence when materializing an existing aggregate.
        typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id))!.SetValue(p,7);
        p.ReserveStock(5);products.Setup(x=>x.GetByIdAsync(7,It.IsAny<CancellationToken>())).ReturnsAsync(p);return p;
    }
    private AdjustInventoryCommandHandler Adjust()=>new(products.Object,movements.Object,tenant.Object,uow.Object,NullLogger<AdjustInventoryCommandHandler>.Instance);
    private AddProductStockCommandHandler Add()=>new(products.Object,movements.Object,tenant.Object,uow.Object,NullLogger<AddProductStockCommandHandler>.Instance);
    [Theory]
    [InlineData(0)] [InlineData(2)]
    public async Task Missing_or_foreign_product_cannot_change_inventory(int owner)
    {
        if(owner>0)Existing(owner);
        (await Adjust().Handle(new(7,10,"Contagem"),default)).Error.Code.Should().Be("Product.NotFound");
        (await Add().Handle(new(7,10,"Compra"),default)).Error.Code.Should().Be("Product.NotFound");
        uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Never);
        movements.Verify(x=>x.AddAsync(It.IsAny<StockMovement>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory]
    [InlineData(-1)] [InlineData(4)]
    public async Task Adjustment_below_reservations_does_not_commit(int quantity)
    {
        var p=Existing();var result=await Adjust().Handle(new(7,quantity,"Contagem"),default);
        result.Error.Code.Should().Be("Product.AdjustmentFailed");p.PhysicalStock.Should().Be(20);
        uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory]
    [InlineData(0)] [InlineData(-1)]
    public async Task Nonpositive_receipt_does_not_commit(int quantity)
    {
        var p=Existing();var result=await Add().Handle(new(7,quantity,"Compra"),default);
        result.Error.Code.Should().Be("Product.InvalidStock");p.PhysicalStock.Should().Be(20);
        uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory]
    [InlineData(10)] [InlineData(20)] [InlineData(30)]
    public async Task Adjustment_records_only_real_difference(int quantity)
    {
        var p=Existing();StockMovement? saved=null;
        movements.Setup(x=>x.AddAsync(It.IsAny<StockMovement>(),It.IsAny<CancellationToken>())).Callback<StockMovement,CancellationToken>((m,_)=>saved=m).Returns(Task.CompletedTask);
        (await Adjust().Handle(new(7,quantity,"Contagem"),default)).IsSuccess.Should().BeTrue();
        p.PhysicalStock.Should().Be(quantity);p.ReservedStock.Should().Be(5);
        if(quantity==20)saved.Should().BeNull();else {
            saved!.QuantityChanged.Should().Be(quantity-20);saved.BalanceAfterMovement.Should().Be(quantity);
            saved.ProductId.Should().Be(7);saved.MovementType.Should().Be(StockMovementType.Adjustment);saved.Reason.Should().Be("Contagem");}
        products.Verify(x=>x.Update(p),Times.Once);uow.Verify(x=>x.CommitAsync(It.IsAny<CancellationToken>()),Times.Once);
    }
    [Fact]
    public async Task Receipt_records_audit_and_commits_with_request_token()
    {
        using var cts=new CancellationTokenSource();var p=Existing();StockMovement? saved=null;
        movements.Setup(x=>x.AddAsync(It.IsAny<StockMovement>(),cts.Token)).Callback<StockMovement,CancellationToken>((m,_)=>saved=m).Returns(Task.CompletedTask);
        (await Add().Handle(new(7,3,"Compra"),cts.Token)).IsSuccess.Should().BeTrue();
        p.PhysicalStock.Should().Be(23);saved!.QuantityChanged.Should().Be(3);
        saved.BalanceAfterMovement.Should().Be(23);saved.MovementType.Should().Be(StockMovementType.PurchaseReceipt);
        saved.ProductId.Should().Be(7);saved.Reason.Should().Be("Compra");
        uow.Verify(x=>x.CommitAsync(cts.Token),Times.Once);
    }
}
