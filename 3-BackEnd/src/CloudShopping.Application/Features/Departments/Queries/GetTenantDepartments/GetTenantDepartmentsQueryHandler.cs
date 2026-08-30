using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Departments.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Departments.Queries.GetTenantDepartments
{
    internal sealed class GetTenantDepartmentsQueryHandler : IRequestHandler<GetTenantDepartmentsQuery, Result<IEnumerable<DepartmentViewModel>>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetTenantDepartmentsQueryHandler(IDepartmentRepository departmentRepository, ITenantProvider tenantProvider)
        {
            _departmentRepository = departmentRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<IEnumerable<DepartmentViewModel>>> Handle(GetTenantDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _departmentRepository.GetAllByTenantAsync(_tenantProvider.GetTenantId(), cancellationToken);
            var viewModels = departments.Select(d => new DepartmentViewModel(
                d.Id,
                d.Name,
                d.Slug,
                d.IsSystemDefault
            ));
            return Result.Success(viewModels);
        }
    }
}