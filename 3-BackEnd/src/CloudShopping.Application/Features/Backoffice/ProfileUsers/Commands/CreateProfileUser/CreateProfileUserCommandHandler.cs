using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.CreateProfileUser
{
    public sealed class CreateProfileUserCommandHandler : IRequestHandler<CreateProfileUserCommand, Result<int>>
    {
        private readonly IProfileUserRepository _profileUserRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProfileUserCommandHandler(
            IProfileUserRepository profileUserRepository,
            IProfileRepository profileRepository,
            IEmployeeUserRepository employeeUserRepository,
            IUnitOfWork unitOfWork)
        {
            _profileUserRepository = profileUserRepository;
            _profileRepository = profileRepository;
            _employeeUserRepository = employeeUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateProfileUserCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(request.ProfileId, cancellationToken);
            if (profile is null || profile.TenantId != request.TenantId)
            {
                return Result<int>.Failure(new Error("Profile.NotFound", "Perfil não encontrado."));
            }

            var employeeUser = await _employeeUserRepository.GetByIdAsync(request.EmployeeUserId, cancellationToken);
            if (employeeUser is null || employeeUser.TenantId != request.TenantId)
            {
                return Result<EmployeeResponse>.Failure(new Error("Employee.NotFound", "Usuário do funcionário não encontrado."));
            }

            var existing = await _profileUserRepository.GetByProfileAndUserAsync(request.TenantId, request.ProfileId, request.EmployeeUserId, cancellationToken);
            if (existing is not null)
            {
                return Result<int>.Failure(new Error("ProfileUser.AlreadyExists", "Este usuário já possui este perfil vinculado."));
            }

            var profileUser = ProfileUser.Create(request.TenantId, request.ProfileId, request.EmployeeUserId);

            await _profileUserRepository.AddAsync(profileUser, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<int>.Success(profileUser.Id);
        }
    }
}