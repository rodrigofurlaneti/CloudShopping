namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant
{
    public sealed record ProfileResponse(
        int Id,
        int TenantId,
        string Name,
        bool IsActive
    );
}