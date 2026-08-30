using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.RegisterB2B
{
    public sealed record RegisterB2BCommand(int CustomerId, string BusinessTaxId, string CompanyName, string? StateTaxId) : IRequest<Result>;

    public sealed class RegisterB2BCommandHandler : IRequestHandler<RegisterB2BCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterB2BCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RegisterB2BCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));

            try
            {
                customer.RegisterAsB2B(request.BusinessTaxId, request.CompanyName, request.StateTaxId);
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
