using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUserById
{
    public sealed class GetProfileUserByIdQueryHandler : IRequestHandler<GetProfileUserByIdQuery, Result<ProfileUserResponse>>
    {
        private readonly IProfileUserRepository _profileUserRepository;
        public GetProfileUserByIdQueryHandler(IProfileUserRepository profileUserRepository)
        {
            _profileUserRepository = profileUserRepository;
        }
        public async Task<Result<ProfileUserResponse>> Handle(GetProfileUserByIdQuery request, CancellationToken cancellationToken)
        {
            var pu = await _profileUserRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pu is null || pu.TenantId != request.TenantId)
            {
                return Result.Failure<ProfileUserResponse>(new Error("ProfileUser.NotFound", "Associação de perfil não encontrada."));
            }
            var response = new ProfileUserResponse(pu.Id, pu.TenantId, pu.ProfileId, pu.EmployeeUserId, pu.IsActive);
            return Result<ProfileUserResponse>.Success(response);
        }
    }
}
