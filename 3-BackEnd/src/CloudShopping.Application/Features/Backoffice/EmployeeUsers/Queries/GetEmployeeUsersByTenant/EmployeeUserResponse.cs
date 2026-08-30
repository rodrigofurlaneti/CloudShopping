namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant
{
    public sealed record EmployeeUserResponse(
        int Id,
        int TenantId,
        int EmployeeId,
        string Username,
        bool IsActive
    );
}