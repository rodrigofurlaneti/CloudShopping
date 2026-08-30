using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Departments.Commands.UpdateDepartment
{
    internal sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;
        public UpdateDepartmentCommandHandler(
            IDepartmentRepository departmentRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider)
        {
            _departmentRepository = departmentRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var department = await _departmentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (department is null || (department.TenantId != tenantId && department.TenantId != null))
                return Result.Failure(new Error("Department.NotFound", "Departamento não encontrado."));
            if (department.IsSystemDefault)
                return Result.Failure(new Error("Department.SystemDefault", "Não é possível alterar um departamento padrão do sistema."));
            if (department.Slug != request.Slug && await _departmentRepository.SlugExistsAsync(tenantId, request.Slug, cancellationToken))
                return Result.Failure(new Error("Department.SlugNotUnique", "Já existe um departamento com este slug."));
            department.Update(request.Name, request.Slug);
            _departmentRepository.Update(department);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}
