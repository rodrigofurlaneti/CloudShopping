using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant
{
    public sealed class GetProfileUsersByTenantQueryHandler : IRequestHandler<GetProfileUsersByTenantQuery, Result<IEnumerable<ProfileUserResponse>>>
    {
        private readonly IProfileUserRepository _profileUserRepository;
        public GetProfileUsersByTenantQueryHandler(IProfileUserRepository profileUserRepository)
        {
            _profileUserRepository = profileUserRepository;
        }
        public async Task<Result<IEnumerable<ProfileUserResponse>>> Handle(GetProfileUsersByTenantQuery request, CancellationToken cancellationToken)
        {
            var relations = await _profileUserRepository.GetAllByTenantAsync(request.TenantId, cancellationToken);
            var response = relations.Select(pu => new ProfileUserResponse(
                pu.Id,
                pu.TenantId,
                pu.ProfileId,
                pu.EmployeeUserId,
                pu.IsActive
            ));
            return Result.Success(response);
        }
    }
}