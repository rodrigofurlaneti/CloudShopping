using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;

namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.UpdateEmployee
{
    public sealed record UpdateEmployeeCommand(
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
    ) : IRequest<Result>;
}