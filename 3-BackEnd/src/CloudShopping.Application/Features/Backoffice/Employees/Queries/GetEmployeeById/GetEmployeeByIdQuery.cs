using CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeeById
{
    public sealed record GetEmployeeByIdQuery(int Id, int TenantId) : IRequest<Result<EmployeeResponse>>;
}