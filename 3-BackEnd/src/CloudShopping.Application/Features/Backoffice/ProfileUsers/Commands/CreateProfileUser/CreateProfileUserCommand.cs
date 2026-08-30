using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.CreateProfileUser
{
    public sealed record CreateProfileUserCommand(int TenantId, int ProfileId, int EmployeeUserId) : IRequest<Result<int>>;
}