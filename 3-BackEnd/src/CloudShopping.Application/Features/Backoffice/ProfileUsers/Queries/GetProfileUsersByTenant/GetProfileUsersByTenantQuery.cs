using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant
{
    public sealed record GetProfileUsersByTenantQuery(int TenantId) : IRequest<Result<IEnumerable<ProfileUserResponse>>>;
}