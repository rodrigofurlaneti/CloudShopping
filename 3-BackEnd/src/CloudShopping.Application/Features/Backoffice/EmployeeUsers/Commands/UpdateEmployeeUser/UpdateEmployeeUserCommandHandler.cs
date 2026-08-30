using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.UpdateEmployeeUser
{
    public sealed class UpdateEmployeeUserCommandHandler : IRequestHandler<UpdateEmployeeUserCommand, Result>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateEmployeeUserCommandHandler(
            IEmployeeUserRepository employeeUserRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _employeeUserRepository = employeeUserRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateEmployeeUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _employeeUserRepository.GetByIdAsync(request.Id, cancellationToken);
            if (user is null || user.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("EmployeeUser.NotFound", "Usuário do backoffice não encontrado."));
            }

            var existingWithUsername = await _employeeUserRepository.GetByUsernameAsync(request.TenantId, request.Username, cancellationToken);
            if (existingWithUsername is not null && existingWithUsername.Id != request.Id)
            {
                return Result.Failure(new Error("EmployeeUser.UsernameTaken", "Já existe outro usuário utilizando este nome de login."));
            }

            user.UpdateDetails(request.Username, request.IsActive);

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                string newPasswordHash = _passwordHasher.Hash(request.NewPassword);
                user.UpdatePassword(newPasswordHash);
            }

            _employeeUserRepository.Update(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}