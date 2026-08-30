using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.UpdateProfile
{
    public sealed record UpdateProfileCommand(int Id, int TenantId, string Name, bool IsActive) : IRequest<Result>;
}