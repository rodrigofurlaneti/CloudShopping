using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using DomainOrderStatus = CloudShopping.Domain.Entities.Orders.OrderStatus;

namespace CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus
{
    public sealed class CreateOrderStatusCommandHandler : IRequestHandler<CreateOrderStatusCommand, Result<int>>
    {
        // Repositório dedicado ao OrderStatus não existe ainda no projeto original;
        // reutiliza-se o DbContext através de um IRepository genérico registrado no DI.
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderStatusCommandHandler(IOrderStatusRepository orderStatusRepository, ITenantProvider tenantProvider, IUnitOfWork unitOfWork)
        {
            _orderStatusRepository = orderStatusRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
        }

        public Task<Result<int>> Handle(CreateOrderStatusCommand request, CancellationToken cancellationToken) => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);
    private async Task<int> ExecuteAsync(CreateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var status = DomainOrderStatus.Create(tenantId, request.OrderSectorId, request.Name);

            await _orderStatusRepository.AddAsync(status, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return status.Id;
        }
    }
}
