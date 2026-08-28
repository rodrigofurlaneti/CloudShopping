using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class RegisterGuestCommandHandler : IRequestHandler<RegisterGuestCommand, Result<RegisterGuestResponse>>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        public RegisterGuestCommandHandler(
            ITenantProvider tenantProvider,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork)
        {
            _tenantProvider = tenantProvider;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<RegisterGuestResponse>> Handle(RegisterGuestCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var guest = Customer.CreateGuest(tenantId);
            await _customerRepository.AddAsync(guest, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            var response = new RegisterGuestResponse(guest.Id, guest.SessionToken);
            return Result.Success(response);
        }
    }
}