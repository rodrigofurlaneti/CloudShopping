using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.UpdateProfileUser
{
    public sealed class UpdateProfileUserCommandHandler : IRequestHandler<UpdateProfileUserCommand, Result>
    {
        private readonly IProfileUserRepository _profileUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileUserCommandHandler(IProfileUserRepository profileUserRepository, IUnitOfWork unitOfWork)
        {
            _profileUserRepository = profileUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var pu = await _profileUserRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pu is null || pu.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("ProfileUser.NotFound", "Associação de perfil não encontrada."));
            }

            pu.UpdateDetails(request.IsActive);

            _profileUserRepository.Update(pu);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}