using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Departments.Commands.DeleteDepartment
{
    public sealed record DeleteDepartmentCommand(int Id) : IRequest<Result>;
}
