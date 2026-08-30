using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.DeleteProfile
{
    public sealed record DeleteProfileCommand(int Id, int TenantId) : IRequest<Result>;
}