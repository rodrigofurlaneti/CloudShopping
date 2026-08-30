using CloudShopping.Application.Abstractions.Data;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Tenants.Queries.GetTenantById
{
    public sealed class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantViewModel?>
    {
        private readonly ITenantRepository _tenantRepository;

        public GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<TenantViewModel?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
            if (tenant is null) return null;

            return new TenantViewModel(tenant.Id, tenant.CompanyName, tenant.Domain);
        }
    }
}
