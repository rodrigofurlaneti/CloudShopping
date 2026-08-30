using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant
{
    public sealed record GetEmployeesByTenantQuery(int TenantId) : IRequest<Result<IEnumerable<EmployeeResponse>>>;
}