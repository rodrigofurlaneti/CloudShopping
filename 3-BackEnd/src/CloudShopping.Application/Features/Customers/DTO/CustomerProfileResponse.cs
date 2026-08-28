using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Customers.DTO
{
    public sealed record CustomerProfileResponse(
        int Id,
        string? Email,
        CustomerType CustomerType,
        string? DocumentNumber,
        string? DisplayName
    );
}
