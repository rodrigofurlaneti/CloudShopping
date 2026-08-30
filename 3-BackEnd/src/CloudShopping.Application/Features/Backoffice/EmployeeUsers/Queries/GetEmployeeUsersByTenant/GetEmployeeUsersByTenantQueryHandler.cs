using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant
{
    public sealed class GetEmployeeUsersByTenantQueryHandler : IRequestHandler<GetEmployeeUsersByTenantQuery, Result<IEnumerable<EmployeeUserResponse>>>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        public GetEmployeeUsersByTenantQueryHandler(IEmployeeUserRepository employeeUserRepository)
        {
            _employeeUserRepository = employeeUserRepository;
        }
        public async Task<Result<IEnumerable<EmployeeUserResponse>>> Handle(GetEmployeeUsersByTenantQuery request, CancellationToken cancellationToken)
        {
            var users = await _employeeUserRepository.GetAllByTenantAsync(request.TenantId, cancellationToken);
            var response = users.Select(u => new EmployeeUserResponse(
                u.Id,
                u.TenantId,
                u.EmployeeId,
                u.Username,
                u.IsActive
            ));
            return Result<IEnumerable<EmployeeUserResponse>>.Success(response);
        }
    }
}