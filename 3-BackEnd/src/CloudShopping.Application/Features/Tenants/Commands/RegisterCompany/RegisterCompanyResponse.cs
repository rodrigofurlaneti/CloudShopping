namespace CloudShopping.Application.Features.Tenants.Commands.RegisterCompany
{
    public sealed record RegisterCompanyResponse(
        int TenantId,
        string CompanyName,
        int EmployeeUserId,
        string Username);
}
