using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using DomainOrderSector = CloudShopping.Domain.Entities.Orders.OrderSector;

namespace CloudShopping.Application.Features.OrderSector.Commands.CreateOrderSector
{
    public sealed class CreateOrderSectorCommandHandler : IRequestHandler<CreateOrderSectorCommand, Result<int>>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderSectorCommandHandler(IOrderSectorRepository orderSectorRepository, ITenantProvider tenantProvider, IUnitOfWork unitOfWork)
        {
            _orderSectorRepository = orderSectorRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateOrderSectorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();

            DomainOrderSector sector;
            try
            {
                sector = DomainOrderSector.Create(tenantId, request.Name);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<int>(new Error("OrderSector.InvalidData", ex.Message));
            }

            await _orderSectorRepository.AddAsync(sector, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(sector.Id);
        }
    }
}
