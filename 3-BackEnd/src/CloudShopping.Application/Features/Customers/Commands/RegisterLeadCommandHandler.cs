using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class RegisterLeadCommandHandler : IRequestHandler<RegisterLeadCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterLeadCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider,
            IPasswordHasher passwordHasher)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> Handle(RegisterLeadCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            bool emailExists = await _customerRepository.EmailExistsAsync(tenantId, request.Email, cancellationToken);
            if (emailExists)
                return Result.Failure(new Error("Customer.EmailAlreadyExists", "Este e-mail já está cadastrado nesta loja."));
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            if (customer.TenantId != tenantId)
                return Result.Failure(new Error("Customer.Unauthorized", "Acesso não autorizado a este cliente."));
            try
            {
                customer.ConvertToLead(request.Email);
                string passwordHash = _passwordHasher.Hash(request.Password);
                customer.SetPassword(passwordHash);
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