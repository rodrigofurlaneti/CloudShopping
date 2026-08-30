using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.DeleteEmployeeUser
{
    public sealed record DeleteEmployeeUserCommand(int Id, int TenantId) : IRequest<Result>;
}