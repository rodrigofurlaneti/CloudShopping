using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed class GetCustomerByIdQueryHandler
        : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDetailsResponse>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetCustomerByIdQueryHandler(
            ICustomerRepository customerRepository,
            ITenantProvider tenantProvider)
        {
            _customerRepository = customerRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<CustomerDetailsResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<CustomerDetailsResponse>(new Error("Customer.NotFound", "Cliente não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (customer.TenantId != tenantId)
                return Result.Failure<CustomerDetailsResponse>(new Error("Customer.Unauthorized", "Você não tem permissão para acessar este cliente."));
            string? document = null;
            string? displayName = null;
            if (customer.CustomerTypeId == CustomerType.B2C && customer.Individual != null)
            {
                document = customer.Individual.TaxId;
                displayName = customer.Individual.FullName;
            }
            else if (customer.CustomerTypeId == CustomerType.B2B && customer.Company != null)
            {
                document = customer.Company.BusinessTaxId;
                displayName = customer.Company.CompanyName;
            }
            var addresses = customer.Addresses.Select(a => new AddressResponse(
                a.Id,
                a.AddressTypeId,
                a.Street,
                a.Number,
                a.City,
                a.State,
                a.ZipCode,
                a.IsDefault
            )).ToList().AsReadOnly();
            var response = new CustomerDetailsResponse(
                customer.Id,
                customer.Email,
                customer.CustomerTypeId,
                document,
                displayName,
                customer.CreatedAt,
                customer.IsActive,
                addresses
            );
            return Result.Success(response);
        }
    }
}
