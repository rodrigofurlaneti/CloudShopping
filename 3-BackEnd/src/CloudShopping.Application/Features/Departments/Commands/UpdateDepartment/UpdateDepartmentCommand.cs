using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Departments.Commands.UpdateDepartment
{
    public sealed record UpdateDepartmentCommand(
            int Id,
            string Name,
            string Slug
        ) : IRequest<Result>;
}
