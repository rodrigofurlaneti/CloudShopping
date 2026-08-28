using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Customers.DTO
{
    public sealed record CustomerSummaryResponse(
        int Id,
        string? Email,
        CustomerType CustomerType,
        DateTime CreatedAt,
        bool IsActive
    );
}
