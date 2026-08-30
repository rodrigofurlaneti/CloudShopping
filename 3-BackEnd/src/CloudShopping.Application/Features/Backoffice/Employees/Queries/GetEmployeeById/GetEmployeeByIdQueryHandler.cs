using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeeById
{
    public sealed class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeResponse>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        public async Task<Result<EmployeeResponse>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
            if (employee is null || employee.TenantId != request.TenantId)
            {
                return Result.Failure<EmployeeResponse>(new Error("Employee.NotFound", "Funcionário não encontrado."));
            }
            var response = new EmployeeResponse(
                employee.Id,
                employee.TenantId,
                employee.Name,
                employee.Cpf,
                employee.Email,
                employee.Phone,
                employee.HiredAt,
                employee.DismissedAt,
                employee.Salary,
                employee.CommissionPercent,
                employee.IsActive
            );
            return Result<EmployeeResponse>.Success(response);
        }
    }
}
