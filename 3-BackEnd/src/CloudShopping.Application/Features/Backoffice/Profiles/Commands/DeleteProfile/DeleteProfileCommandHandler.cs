using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.DeleteProfile
{
    public sealed class DeleteProfileCommandHandler : IRequestHandler<DeleteProfileCommand, Result>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProfileCommandHandler(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
        {
            _profileRepository = profileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.Id, cancellationToken);

            if (profile is null || profile.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("Profile.NotFound", "Perfil não encontrado."));
            }

            _profileRepository.Remove(profile);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}