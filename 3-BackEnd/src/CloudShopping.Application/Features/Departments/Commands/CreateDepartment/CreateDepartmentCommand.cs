using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Departments.Commands.CreateDepartment
{
    public sealed record CreateDepartmentCommand(
            string Name,
            string Slug
        ) : IRequest<Result<int>>; 
}
