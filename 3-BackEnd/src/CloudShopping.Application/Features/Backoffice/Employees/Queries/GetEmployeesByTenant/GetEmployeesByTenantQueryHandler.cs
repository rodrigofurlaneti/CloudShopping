using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant
{
    public sealed class GetEmployeesByTenantQueryHandler : IRequestHandler<GetEmployeesByTenantQuery, Result<IEnumerable<EmployeeResponse>>>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetEmployeesByTenantQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
        public async Task<Result<IEnumerable<EmployeeResponse>>> Handle(GetEmployeesByTenantQuery request, CancellationToken cancellationToken)
        {
            var employees = await _employeeRepository.GetAllByTenantAsync(request.TenantId, cancellationToken);
            var response = employees.Select(e => new EmployeeResponse(
                e.Id,
                e.TenantId,
                e.Name,
                e.Cpf,
                e.Email,
                e.Phone,
                e.HiredAt,
                e.DismissedAt,
                e.Salary,
                e.CommissionPercent,
                e.IsActive
            ));
            return Result<IEnumerable<EmployeeResponse>>.Success(response);
        }
    }
}