using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.ChangeCustomerEmail
{
    public sealed record ChangeCustomerEmailCommand(int CustomerId, string NewEmail) : IRequest<Result>;

    public sealed class ChangeCustomerEmailCommandHandler : IRequestHandler<ChangeCustomerEmailCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ChangeCustomerEmailCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ChangeCustomerEmailCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));

            try
            {
                customer.ChangeEmail(request.NewEmail);
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
