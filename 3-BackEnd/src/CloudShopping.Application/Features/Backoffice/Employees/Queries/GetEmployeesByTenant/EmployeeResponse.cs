using System;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant
{
    public sealed record EmployeeResponse(
        int Id,
        int TenantId,
        string Name,
        string Cpf,
        string? Email,
        string? Phone,
        DateTime HiredAt,
        DateTime? DismissedAt,
        decimal? Salary,
        decimal? CommissionPercent,
        bool IsActive
    );
}