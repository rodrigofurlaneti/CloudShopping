using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Commands.CreateProfile
{
    public sealed class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, Result<int>>
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProfileCommandHandler(IProfileRepository profileRepository, IUnitOfWork unitOfWork)
        {
            _profileRepository = profileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
        {
            var existing = await _profileRepository.GetByNameAsync(request.TenantId, request.Name, cancellationToken);
            if (existing is not null)
            {
                return Result<int>.Failure(new Error("Profile.AlreadyExists", "Já existe um perfil cadastrado com este nome para esta empresa."));
            }

            var profile = Profile.Create(request.TenantId, request.Name);

            await _profileRepository.AddAsync(profile, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<int>.Success(profile.Id);
        }
    }
}