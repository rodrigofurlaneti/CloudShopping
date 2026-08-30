using CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUserById
{
    public sealed record GetProfileUserByIdQuery(int Id, int TenantId) : IRequest<Result<ProfileUserResponse>>;
}