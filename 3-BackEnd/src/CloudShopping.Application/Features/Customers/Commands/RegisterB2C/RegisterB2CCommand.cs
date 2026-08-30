using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.RegisterB2C
{
    public sealed record RegisterB2CCommand(int CustomerId, string TaxId, string FullName, DateTime? BirthDate) : IRequest<Result>;

    public sealed class RegisterB2CCommandHandler : IRequestHandler<RegisterB2CCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterB2CCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RegisterB2CCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));

            try
            {
                customer.RegisterAsB2C(request.TaxId, request.FullName, request.BirthDate);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Customer.InvalidOperation", ex.Message));
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
