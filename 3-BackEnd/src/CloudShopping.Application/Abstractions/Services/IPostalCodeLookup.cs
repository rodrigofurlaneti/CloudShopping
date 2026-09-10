namespace CloudShopping.Application.Abstractions.Services;
public sealed record PostalAddress(string ZipCode, string Street, string Neighborhood, string City, string State);
public interface IPostalCodeLookup
{
    Task<PostalAddress?> FindAsync(string zipCode, CancellationToken ct);
}
