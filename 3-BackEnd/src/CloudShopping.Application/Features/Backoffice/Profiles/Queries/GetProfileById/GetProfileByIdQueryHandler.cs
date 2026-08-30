using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfileById
{
    public sealed class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, Result<ProfileResponse>>
    {
        private readonly IProfileRepository _profileRepository;

        public GetProfileByIdQueryHandler(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<Result<ProfileResponse>> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.Id, cancellationToken);

            if (profile is null || profile.TenantId != request.TenantId)
            {
                return Result<ProfileResponse>.Failure(new Error("Profile.NotFound", "Perfil não encontrado."));
            }

            var response = new ProfileResponse(profile.Id, profile.TenantId, profile.Name, profile.IsActive);

            return Result<ProfileResponse>.Success(response);
        }
    }
}