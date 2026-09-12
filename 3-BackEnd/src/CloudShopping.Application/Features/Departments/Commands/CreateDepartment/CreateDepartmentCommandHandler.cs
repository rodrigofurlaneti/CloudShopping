using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Departments.Commands.CreateDepartment
{
    internal sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<int>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICacheService _cache;

        public CreateDepartmentCommandHandler(
            IDepartmentRepository departmentRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider,
            ICacheService cache)
        {
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
            _cache = cache;
        }

        public async Task<Result<int>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            if (await _departmentRepository.SlugExistsAsync(tenantId, request.Slug, cancellationToken))
            {
                return Result.Failure<int>(new Error(
                    "Department.SlugNotUnique",
                    "Já existe um departamento com este slug neste tenant."));
            }

            var department = Department.CreateForTenant(tenantId, request.Name, request.Slug);
            await _departmentRepository.AddAsync(department, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            await _cache.RemoveAsync(CacheKeys.TenantList(tenantId, CacheKeys.Departments), cancellationToken);

            return Result.Success(department.Id);
        }
    }
}
