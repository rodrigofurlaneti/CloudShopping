using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, Result<CustomerProfileResponse>>
    {
        private readonly ICustomerRepository _customerRepository;
        public GetCustomerProfileQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        public async Task<Result<CustomerProfileResponse>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<CustomerProfileResponse>(new Error("Customer.NotFound", "Cliente não encontrado."));
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
            var response = new CustomerProfileResponse(
                customer.Id,
                customer.Email,
                customer.CustomerTypeId,
                document,
                displayName
            );
            return Result.Success(response);
        }
    }
}