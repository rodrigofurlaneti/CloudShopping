using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetPaginatedCustomers
{
    public sealed class GetPaginatedCustomersQueryHandler
        : IRequestHandler<GetPaginatedCustomersQuery, Result<PagedResult<CustomerSummaryResponse>>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetPaginatedCustomersQueryHandler(
            ICustomerRepository customerRepository,
            ITenantProvider tenantProvider)
        {
            _customerRepository = customerRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<PagedResult<CustomerSummaryResponse>>> Handle(
            GetPaginatedCustomersQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var (items, totalCount) = await _customerRepository.GetPaginatedAsync(
                tenantId,
                request.Page,
                request.PageSize,
                request.SearchTerm,
                cancellationToken);
            var responseItems = items.Select(c => new CustomerSummaryResponse(
                c.Id,
                c.Email,
                c.CustomerTypeId,
                c.CreatedAt,
                c.IsActive
            )).ToList().AsReadOnly();
            var pagedResult = new PagedResult<CustomerSummaryResponse>(
                responseItems,
                totalCount,
                request.Page,
                request.PageSize);
            return Result.Success(pagedResult);
        }
    }
}