using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.RegisterGuest
{
    public sealed class RegisterGuestCommandHandler : IRequestHandler<RegisterGuestCommand, Result<int>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterGuestCommandHandler(ICustomerRepository customerRepository, ITenantProvider tenantProvider, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
        }

        public Task<Result<int>> Handle(RegisterGuestCommand request, CancellationToken cancellationToken) => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);
    private async Task<int> ExecuteAsync(RegisterGuestCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var customer = Customer.CreateGuest(tenantId);

            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return customer.Id;
        }
    }
}
