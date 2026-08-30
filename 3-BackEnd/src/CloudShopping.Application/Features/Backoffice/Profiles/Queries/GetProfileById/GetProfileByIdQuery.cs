using CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfileById
{
    public sealed record GetProfileByIdQuery(int Id, int TenantId) : IRequest<Result<ProfileResponse>>;
}