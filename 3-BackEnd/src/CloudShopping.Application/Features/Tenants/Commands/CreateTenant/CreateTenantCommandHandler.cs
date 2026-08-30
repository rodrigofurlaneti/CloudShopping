using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Tenants.Commands.CreateTenant
{
    public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<int>>
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTenantCommandHandler(ITenantRepository tenantRepository, IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            Tenant tenant;
            try
            {
                tenant = Tenant.Create(request.CompanyName, request.Domain);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<int>(new Error("Tenant.InvalidData", ex.Message));
            }

            await _tenantRepository.AddAsync(tenant, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(tenant.Id);
        }
    }
}
