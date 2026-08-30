using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.DeleteProfileUser
{
    public sealed class DeleteProfileUserCommandHandler : IRequestHandler<DeleteProfileUserCommand, Result>
    {
        private readonly IProfileUserRepository _profileUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProfileUserCommandHandler(IProfileUserRepository profileUserRepository, IUnitOfWork unitOfWork)
        {
            _profileUserRepository = profileUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteProfileUserCommand request, CancellationToken cancellationToken)
        {
            var pu = await _profileUserRepository.GetByIdAsync(request.Id, cancellationToken);
            if (pu is null || pu.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("ProfileUser.NotFound", "Associação de perfil não encontrada."));
            }

            _profileUserRepository.Remove(pu);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}   