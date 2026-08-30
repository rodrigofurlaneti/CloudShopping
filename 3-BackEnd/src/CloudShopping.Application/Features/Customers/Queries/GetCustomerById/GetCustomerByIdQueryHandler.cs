using CloudShopping.Application.Abstractions.Data;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerViewModel?>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerViewModel?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null) return null;

            return new CustomerViewModel(
                customer.Id,
                customer.Email,
                customer.CustomerTypeId.ToString(),
                customer.Individual?.FullName,
                customer.Company?.CompanyName,
                customer.Addresses.Select(a => new CustomerAddressViewModel(a.Id, a.Street, a.Number, a.Neighborhood, a.City, a.State, a.ZipCode, a.IsDefault)).ToList());
        }
    }
}
