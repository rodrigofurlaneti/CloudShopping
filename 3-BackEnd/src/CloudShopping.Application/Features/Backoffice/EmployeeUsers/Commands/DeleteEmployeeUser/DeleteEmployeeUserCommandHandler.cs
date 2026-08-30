using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.DeleteEmployeeUser
{
    public sealed class DeleteEmployeeUserCommandHandler : IRequestHandler<DeleteEmployeeUserCommand, Result>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEmployeeUserCommandHandler(
            IEmployeeUserRepository employeeUserRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeUserRepository = employeeUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteEmployeeUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _employeeUserRepository.GetByIdAsync(request.Id, cancellationToken);

            if (user is null || user.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("EmployeeUser.NotFound", "Usuário do backoffice não encontrado."));
            }

            _employeeUserRepository.Remove(user);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}