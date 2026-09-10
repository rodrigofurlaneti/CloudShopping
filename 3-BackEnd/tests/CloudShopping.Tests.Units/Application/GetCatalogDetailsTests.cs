using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.Queries.GetCatalogDetails;
using CloudShopping.Domain.Entities.Products;
using Moq;
using Xunit;
namespace CloudShopping.Tests.Units.Application;
public class GetCatalogDetailsTests
{
    private readonly Mock<IProductRepository> repository = new(MockBehavior.Strict);
    private readonly Mock<ITenantProvider> tenant = new();
    private GetCatalogDetailsQueryHandler Handler() { tenant.Setup(x => x.GetTenantId()).Returns(1); return new(repository.Object, tenant.Object); }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Returns_success_for_empty_or_populated_catalog(bool populated)
    {
        var products = populated ? new[] { Product.Create(1, 1, "SKU", "Produto", 10) } : Array.Empty<Product>();
        using var source = new CancellationTokenSource();
        repository.Setup(x => x.GetPaginatedAsync(1, 1, 20, null, source.Token)).ReturnsAsync(((IEnumerable<Product>)products, products.Length));
        var result = await Handler().Handle(new(1), source.Token);
        Assert.True(result.IsSuccess);
        Assert.Equal(products.Length, result.Value.Count);
        if (populated) Assert.Equal("SKU", result.Value[0].Sku);
        repository.VerifyAll();
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task Invalid_page_returns_failure_without_query(int page)
    {
        var result = await Handler().Handle(new(page), default);
        Assert.True(result.IsFailure);
        Assert.Equal("Catalog.InvalidPage", result.Error.Code);
        repository.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Cancellation_is_not_converted_to_success()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Handler().Handle(new(1), source.Token));
        repository.VerifyNoOtherCalls();
    }
    [Fact]
    public async Task Repository_cancellation_is_preserved()
    {
        var failure = new OperationCanceledException();
        repository.Setup(x => x.GetPaginatedAsync(1, 1, 20, null, default)).ThrowsAsync(failure);
        var thrown = await Record.ExceptionAsync(() => Handler().Handle(new(1), default));
        Assert.Same(failure, thrown);
    }
}
