using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.UpdateEmployee
{
    public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (employee is null || employee.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("Employee.NotFound", "Funcionário não encontrado."));

            }
            var existingWithCpf = await _employeeRepository.GetByCpfAsync(request.TenantId, request.Cpf, cancellationToken);
            if (existingWithCpf is not null && existingWithCpf.Id != request.Id)
            {
                return Result.Failure(new Error("Employee.NotFound", "Já existe outro funcionário cadastrado com este CPF."));
            }
            employee.UpdateDetails(
                request.Name,
                request.Cpf,
                request.Email,
                request.Phone,
                request.HiredAt,
                request.DismissedAt,
                request.Salary,
                request.CommissionPercent,
                request.IsActive
            );
            await _employeeRepository.Update(employee);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}