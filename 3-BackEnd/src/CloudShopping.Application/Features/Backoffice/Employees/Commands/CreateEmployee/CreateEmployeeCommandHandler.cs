using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.CreateEmployee
{
    public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CreateEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var existingEmployee = await _employeeRepository.GetByCpfAsync(request.TenantId, request.Cpf, cancellationToken);
            if (existingEmployee is not null)
            {
                return Result.Failure<int>(new Error("Employee.NotValidCpf", "Já existe um funcionário cadastrado com este CPF para esta empresa."));
            }
            var employee = Employee.Create(
                request.TenantId,
                request.Name,
                request.Cpf,
                request.Email,
                request.Phone,
                request.HiredAt,
                request.Salary,
                request.CommissionPercent
            );
            await _employeeRepository.AddAsync(employee, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<int>.Success(employee.Id);
        }
    }
}
