using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUserById
{
    public sealed record GetEmployeeUserByIdQuery(int Id, int TenantId) : IRequest<Result<EmployeeUserResponse>>;
}
