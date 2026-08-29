namespace CloudShopping.Application.Features.Orders.DTO
{
    public record AddressDto(int AddressTypeId, string Street, string Number, string Neighborhood, string City, string State, string ZipCode);
}
