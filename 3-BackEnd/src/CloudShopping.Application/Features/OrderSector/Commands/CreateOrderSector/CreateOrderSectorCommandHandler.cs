using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderSector.Commands.CreateOrderSector
{
    public sealed class CreateOrderSectorCommandHandler : IRequestHandler<CreateOrderSectorCommand, Result<int>>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderSectorCommandHandler> _logger;

        public CreateOrderSectorCommandHandler(
            IOrderSectorRepository orderSectorRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderSectorCommandHandler> logger)
        {
            _orderSectorRepository = orderSectorRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateOrderSectorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var sector = OrderSector.Create(request.Name);
                await _orderSectorRepository.AddAsync(sector, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Novo setor de Kanban criado: {SectorName} (Id: {SectorId})", sector.Name, sector.Id);
                return Result.Success(sector.Id);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<int>(new Error("OrderSector.Invalid", ex.Message));
            }
        }
    }
}
