using MediatR;
using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // Necessário para o ITenantProvider

namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class RegisterB2BCommandHandler : IRequestHandler<RegisterB2BCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider; // 1. Adicionado para segurança multi-tenant

        public RegisterB2BCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider) // 2. Injetado no construtor
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result> Handle(RegisterB2BCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (customer.TenantId != tenantId)
                return Result.Failure(new Error("Customer.Unauthorized", "Você não tem permissão para alterar este cliente."));
            try
            {
                customer.RegisterAsB2B(request.BusinessTaxId, request.CompanyName, request.StateTaxId);
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