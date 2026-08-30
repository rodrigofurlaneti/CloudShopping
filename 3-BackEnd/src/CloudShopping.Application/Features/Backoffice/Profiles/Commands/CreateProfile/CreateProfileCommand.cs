using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.CreateProfile
{
    public sealed record CreateProfileCommand(int TenantId, string Name) : IRequest<Result<int>>;
}