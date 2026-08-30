using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.AddCustomerAddress
{
    public sealed record AddCustomerAddressCommand(
        int CustomerId,
        AddressType AddressTypeId,
        string Street,
        string Number,
        string City,
        string State,
        string ZipCode,
        bool IsDefault) : IRequest<Result>;

    public sealed class AddCustomerAddressCommandHandler : IRequestHandler<AddCustomerAddressCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddCustomerAddressCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));

            try
            {
                customer.AddAddress(request.AddressTypeId, request.Street, request.Number, request.City, request.State, request.ZipCode, request.IsDefault);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(new Error("Customer.InvalidData", ex.Message));
            }

            _customerRepository.Update(customer);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
