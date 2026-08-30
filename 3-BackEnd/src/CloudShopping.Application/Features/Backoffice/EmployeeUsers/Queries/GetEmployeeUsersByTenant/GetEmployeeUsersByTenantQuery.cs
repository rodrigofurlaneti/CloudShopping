using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant
{
    public sealed record GetEmployeeUsersByTenantQuery(int TenantId) : IRequest<Result<IEnumerable<EmployeeUserResponse>>>;
}