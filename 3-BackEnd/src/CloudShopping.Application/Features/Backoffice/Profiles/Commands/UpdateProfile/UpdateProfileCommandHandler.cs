using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.UpdateProfile
{
    public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileCommandHandler(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
        {
            _profileRepository = profileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.Id, cancellationToken);
            if (profile is null || profile.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("Profile.NotFound", "Perfil não encontrado."));
            }

            var existingWithName = await _profileRepository.GetByNameAsync(request.TenantId, request.Name, cancellationToken);
            if (existingWithName is not null && existingWithName.Id != request.Id)
            {
                return Result.Failure(new Error("Profile.NameTaken", "Já existe outro perfil com este nome."));
            }

            profile.UpdateDetails(request.Name, request.IsActive);

            _profileRepository.Update(profile);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}