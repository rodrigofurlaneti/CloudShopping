using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.UpdateCustomerAddress
{
    public sealed record UpdateCustomerAddressCommand(
        int CustomerId,
        int AddressId,
        AddressType AddressTypeId,
        string Street,
        string Number,
        string City,
        string State,
        string ZipCode,
        bool IsDefault) : IRequest<Result>;

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
                customer.UpdateAddress(request.AddressId, request.AddressTypeId, request.Street, request.Number, request.City, request.State, request.ZipCode, request.IsDefault);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Customer.AddressNotFound", ex.Message));
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
