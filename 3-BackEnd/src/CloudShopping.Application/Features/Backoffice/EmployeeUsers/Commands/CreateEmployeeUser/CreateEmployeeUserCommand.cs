using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.CreateEmployeeUser
{
    public sealed record CreateEmployeeUserCommand(
        int TenantId,
        int EmployeeId,
        string Username,
        string Password
    ) : IRequest<Result<int>>;
}