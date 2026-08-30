using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant
{
    public sealed class GetProfilesByTenantQueryHandler : IRequestHandler<GetProfilesByTenantQuery, Result<IEnumerable<ProfileResponse>>>
    {
        private readonly IProfileRepository _profileRepository;
        public GetProfilesByTenantQueryHandler(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }
        public async Task<Result<IEnumerable<ProfileResponse>>> Handle(GetProfilesByTenantQuery request, CancellationToken cancellationToken)
        {
            var profiles = await _profileRepository.GetAllByTenantAsync(request.TenantId, cancellationToken);
            var response = profiles.Select(p => new ProfileResponse(
                p.Id,
                p.TenantId,
                p.Name,
                p.IsActive
            ));
            return Result<IEnumerable<ProfileResponse>>.Success(response);
        }
    }
}