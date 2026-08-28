using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record CustomerDetailsResponse(
        int Id,
        string? Email,
        CustomerType CustomerType,
        string? DocumentNumber,
        string? DisplayName,
        DateTime CreatedAt,
        bool IsActive,
        IReadOnlyCollection<AddressResponse> Addresses
    );
}
