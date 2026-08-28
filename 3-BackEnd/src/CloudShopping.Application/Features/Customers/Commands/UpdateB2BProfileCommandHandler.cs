using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // 1. Necessário para o ITenantProvider
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class UpdateB2BProfileCommandHandler : IRequestHandler<UpdateB2BProfileCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider; // 2. Injetado para segurança multi-tenant

        public UpdateB2BProfileCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider) // 3. Adicionado no construtor
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result> Handle(UpdateB2BProfileCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (customer.TenantId != tenantId)
                return Result.Failure(new Error("Customer.Unauthorized", "Você não tem permissão para alterar este cliente."));
            try
            {
                customer.UpdateB2BProfile(request.CompanyName, request.StateTaxId);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Customer.InvalidUpdate", ex.Message));
            }
            _customerRepository.Update(customer);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}