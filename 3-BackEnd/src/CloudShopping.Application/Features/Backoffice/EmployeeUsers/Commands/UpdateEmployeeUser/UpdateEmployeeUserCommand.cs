using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.UpdateEmployeeUser
{
    public sealed record UpdateEmployeeUserCommand(
        int Id,
        int TenantId,
        string Username,
        string? NewPassword,
        bool IsActive
    ) : IRequest<Result>;
}