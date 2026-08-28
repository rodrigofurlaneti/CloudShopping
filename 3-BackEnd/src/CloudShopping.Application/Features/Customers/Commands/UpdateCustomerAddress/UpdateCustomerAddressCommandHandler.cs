using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.UpdateCustomerAddress
{
    public sealed class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCustomerAddressCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            try
            {
                customer.UpdateAddress(
                    request.AddressId,
                    request.Street,
                    request.Number,
                    request.City,
                    request.State,
                    request.ZipCode,
                    request.IsDefault);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Address.InvalidUpdate", ex.Message));
            }
            _customerRepository.Update(customer);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}
