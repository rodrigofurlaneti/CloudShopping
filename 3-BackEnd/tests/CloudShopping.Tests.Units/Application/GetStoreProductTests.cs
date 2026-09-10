using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Storefront.Queries.GetStoreProduct;
using CloudShopping.Domain.Entities.Products;
using Moq;
using Xunit;

namespace CloudShopping.Tests.Units.Application;
public class GetStoreProductTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"Cor\":\"Azul\"}")]
    public async Task Reads_legacy_empty_attributes_and_preserves_valid_attributes(string? json)
    {
        var product = Product.Create(1, 1, "SKU", "Produto", 10);
        // Simulate materialization of a legacy database row, bypassing creation defaults.
        typeof(Product).GetProperty(nameof(Product.AttributesJson))!.SetValue(product, json);
        var repository = new Mock<IStorefrontRepository>();
        repository.Setup(x => x.Product(1, null, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        repository.Setup(x => x.Variants(null, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<StoreVariantView>());
        var result = await new GetStoreProductQueryHandler(repository.Object).Handle(new(1, null), default);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(json?.Contains("Azul") == true ? 1 : 0, result.Value!.Attributes.Count);
        if (result.Value!.Attributes.Count > 0) Assert.Equal("Azul", result.Value!.Attributes["Cor"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Catalog_normalizes_empty_attributes_on_write(string json)
    {
        var product = Product.Create(1, 1, "SKU", "Produto", 10);
        product.ConfigureCatalog("produto", "", null, 0, 0, 0, 0, null, null, json);
        Assert.Equal("{}", product.AttributesJson);
    }
}
