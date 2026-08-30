using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.DeleteProfileUser
{
    public sealed record DeleteProfileUserCommand(int Id, int TenantId) : IRequest<Result>;
}