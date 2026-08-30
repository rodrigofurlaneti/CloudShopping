// File: src/CloudShopping.Application/Features/Backoffice/Employees/Commands/DeleteEmployee/DeleteEmployeeCommandHandler.cs
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.DeleteEmployee
{
    public sealed class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);

            if (employee is null || employee.TenantId != request.TenantId)
            {
                return Result.Failure(new Error("Employee.NotFound", "Funcionário não encontrado."));
            }
            _employeeRepository.Remove(employee);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}