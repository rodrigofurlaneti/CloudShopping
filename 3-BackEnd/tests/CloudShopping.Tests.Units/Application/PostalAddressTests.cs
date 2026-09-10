using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Queries.GetPostalAddress;
using Moq;
using Xunit;
namespace CloudShopping.Tests.Units.Application;
public class PostalAddressTests
{
    [Theory]
    [InlineData("01001000", true)]
    [InlineData("", false)]
    [InlineData("01001-000", false)]
    [InlineData("123456789", false)]
    [InlineData("abcdefgh", false)]
    public void Validates_postal_code(string value, bool valid) => Assert.Equal(valid, new GetPostalAddressQueryValidator().Validate(new GetPostalAddressQuery(value)).IsValid);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Preserves_lookup_result_and_token(bool found)
    {
        var lookup = new Mock<IPostalCodeLookup>();
        PostalAddress? expected = found ? new("01001000", "Praça da Sé", "Sé", "São Paulo", "SP") : null;
        using var source = new CancellationTokenSource();
        lookup.Setup(x => x.FindAsync("01001000", source.Token)).ReturnsAsync(expected);
        var result = await new GetPostalAddressQueryHandler(lookup.Object).Handle(new("01001000"), source.Token);
        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
        lookup.VerifyAll();
    }
}

