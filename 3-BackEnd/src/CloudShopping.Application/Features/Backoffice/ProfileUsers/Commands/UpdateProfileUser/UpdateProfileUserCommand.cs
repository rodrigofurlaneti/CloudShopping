using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.UpdateProfileUser
{
    public sealed record UpdateProfileUserCommand(int Id, int TenantId, bool IsActive) : IRequest<Result>;
}