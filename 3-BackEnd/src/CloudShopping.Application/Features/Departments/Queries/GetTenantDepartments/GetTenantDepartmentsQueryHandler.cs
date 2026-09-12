using CloudShopping.Application.Abstractions.Caching;
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
        private readonly ICacheService _cache;

        public GetTenantDepartmentsQueryHandler(IDepartmentRepository departmentRepository, ITenantProvider tenantProvider, ICacheService cache)
        {
            _departmentRepository = departmentRepository;
            _tenantProvider = tenantProvider;
            _cache = cache;
        }

        public async Task<Result<IEnumerable<DepartmentViewModel>>> Handle(GetTenantDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            // Cache Longo (departments, tarefa de cache Redis): a lista muda pouco e é
            // lida em praticamente toda tela do painel/vitrine. Invalidada em
            // Create/Update/DeleteDepartmentCommandHandler.
            var cacheKey = CacheKeys.TenantList(tenantId, CacheKeys.Departments);
            var viewModels = await _cache.GetOrCreateAsync(cacheKey, CacheTtl.Long, async ct =>
            {
                var departments = await _departmentRepository.GetAllByTenantAsync(tenantId, ct);
                return departments.Select(d => new DepartmentViewModel(
                    d.Id,
                    d.Name,
                    d.Slug,
                    d.IsSystemDefault
                )).ToList();
            }, cancellationToken);

            return Result.Success<IEnumerable<DepartmentViewModel>>(viewModels ?? new List<DepartmentViewModel>());
        }
    }
}
