namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record OrderAddressResponse(string Street, string Number, string? Neighborhood, string City, string State, string ZipCode);
}
