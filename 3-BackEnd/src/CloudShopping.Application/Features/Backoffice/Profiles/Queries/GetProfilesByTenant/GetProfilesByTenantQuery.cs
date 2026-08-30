using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant
{
    public sealed record GetProfilesByTenantQuery(int TenantId) : IRequest<Result<IEnumerable<ProfileResponse>>>;
}