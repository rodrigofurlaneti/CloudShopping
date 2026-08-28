namespace CloudShopping.Application.Features.Orders.Queries
{
    public sealed record OrderAddressResponse(string Street, string Number, string? Neighborhood, string City, string State, string ZipCode);
}
