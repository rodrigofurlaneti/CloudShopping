namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant
{
    public sealed record ProfileUserResponse(
        int Id,
        int TenantId,
        int ProfileId,
        int EmployeeUserId,
        bool IsActive
    );
}