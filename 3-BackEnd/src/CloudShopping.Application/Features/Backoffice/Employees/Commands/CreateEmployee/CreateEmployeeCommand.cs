using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.CreateEmployee
{
    public sealed record CreateEmployeeCommand(
        int TenantId,
        string Name,
        string Cpf,
        string? Email,
        string? Phone,
        DateTime HiredAt,
        decimal? Salary,
        decimal? CommissionPercent
    ) : IRequest<Result<int>>;
}