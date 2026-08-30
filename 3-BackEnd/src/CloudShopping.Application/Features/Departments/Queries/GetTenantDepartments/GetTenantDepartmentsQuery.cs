using CloudShopping.Application.Features.Departments.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Departments.Queries.GetTenantDepartments
{
    public sealed record GetTenantDepartmentsQuery() : IRequest<Result<IEnumerable<DepartmentViewModel>>>;
}
