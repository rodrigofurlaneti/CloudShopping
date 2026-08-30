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

        public CreateDepartmentCommandHandler(
            IDepartmentRepository departmentRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider)
        {
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
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
            return Result.Success(department.Id);
        }
    }
}
