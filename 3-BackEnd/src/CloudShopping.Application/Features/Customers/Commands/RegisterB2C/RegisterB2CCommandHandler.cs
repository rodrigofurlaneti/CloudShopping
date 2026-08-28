using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // 1. Necessário para o ITenantProvider
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Customers.Commands.RegisterB2C
{
    public sealed class RegisterB2CCommandHandler : IRequestHandler<RegisterB2CCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;
        public RegisterB2CCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result> Handle(RegisterB2CCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (customer.TenantId != tenantId)
                return Result.Failure(new Error("Customer.Unauthorized", "Você não tem permissão para alterar este cliente."));
            try
            {
                customer.RegisterAsB2C(request.TaxId, request.FullName, request.BirthDate);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Customer.InvalidTransition", ex.Message));
            }
            _customerRepository.Update(customer);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}