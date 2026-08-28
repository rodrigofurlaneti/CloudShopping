using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // 1. Necessário para o ITenantProvider
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Customers.Commands.UpdateB2CProfile
{
    public sealed class UpdateB2CProfileCommandHandler : IRequestHandler<UpdateB2CProfileCommand, Result>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantProvider _tenantProvider;

        public UpdateB2CProfileCommandHandler(
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork,
            ITenantProvider tenantProvider)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result> Handle(UpdateB2CProfileCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure(new Error("Customer.NotFound", "Cliente não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (customer.TenantId != tenantId)
                return Result.Failure(new Error("Customer.Unauthorized", "Você não tem permissão para alterar este cliente."));
            try
            {
                customer.UpdateB2CProfile(request.FullName, request.BirthDate);
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