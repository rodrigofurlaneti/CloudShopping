using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record AddressResponse(
       int Id,
       AddressType AddressType,
       string Street,
       string Number,
       string? Neighborhood,
       string City,
       string State,
       string ZipCode,
       bool IsDefault
   );
}
