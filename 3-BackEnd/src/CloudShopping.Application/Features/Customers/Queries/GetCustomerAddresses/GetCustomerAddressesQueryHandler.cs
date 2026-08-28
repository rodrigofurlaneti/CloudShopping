using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerAddresses
{
    public sealed class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, Result<IReadOnlyCollection<AddressResponse>>>
    {
        private readonly ICustomerRepository _customerRepository;
        public GetCustomerAddressesQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        public async Task<Result<IReadOnlyCollection<AddressResponse>>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<IReadOnlyCollection<AddressResponse>>(new Error("Customer.NotFound", "Cliente não encontrado."));
            var response = customer.Addresses
                .Where(a => a.IsActive)
                .Select(a => new AddressResponse(
                    a.Id,
                    a.AddressTypeId,
                    a.Street,
                    a.Number,
                    a.Neighborhood,
                    a.City,
                    a.State,
                    a.ZipCode,
                    a.IsDefault
                ))
                .ToList()
                .AsReadOnly();
            return Result.Success<IReadOnlyCollection<AddressResponse>>(response);
        }
    }
}