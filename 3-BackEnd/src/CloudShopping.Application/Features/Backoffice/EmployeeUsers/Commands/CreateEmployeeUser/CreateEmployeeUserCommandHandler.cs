using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.CreateEmployeeUser
{
    public sealed class CreateEmployeeUserCommandHandler : IRequestHandler<CreateEmployeeUserCommand, Result<int>>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateEmployeeUserCommandHandler(
            IEmployeeUserRepository employeeUserRepository,
            IEmployeeRepository employeeRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _employeeUserRepository = employeeUserRepository;
            _employeeRepository = employeeRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateEmployeeUserCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
            if (employee is null || employee.TenantId != request.TenantId)
            {
                return Result<int>.Failure(new Error("EmployeeUser.EmployeeNotFound", "Funcionário não encontrado."));
            }

            var existingUser = await _employeeUserRepository.GetByUsernameAsync(request.TenantId, request.Username, cancellationToken);
            if (existingUser is not null)
            {
                return Result<int>.Failure(new Error("EmployeeUser.UsernameTaken", "Já existe um usuário com este nome de login para esta empresa."));
            }

            string passwordHash = _passwordHasher.Hash(request.Password);

            var employeeUser = EmployeeUser.Create(
                request.TenantId,
                request.EmployeeId,
                request.Username,
                passwordHash
            );

            await _employeeUserRepository.AddAsync(employeeUser, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result<int>.Success(employeeUser.Id);
        }
    }
}