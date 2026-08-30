using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
namespace CloudShopping.Application.Features.OrderSector.Commands.ToggleOrderSectorStatus
{
    public sealed class ToggleOrderSectorStatusCommandHandler : IRequestHandler<ToggleOrderSectorStatusCommand, Result>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ToggleOrderSectorStatusCommandHandler> _logger;
        public ToggleOrderSectorStatusCommandHandler(
            IOrderSectorRepository orderSectorRepository,
            IUnitOfWork unitOfWork,
            ILogger<ToggleOrderSectorStatusCommandHandler> logger)
        {
            _orderSectorRepository = orderSectorRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(ToggleOrderSectorStatusCommand request, CancellationToken cancellationToken)
        {
            var sector = await _orderSectorRepository.GetByIdAsync(request.Id, cancellationToken);
            if (sector is null) return Result.Failure(new Error("OrderSector.NotFound", "Setor não encontrado."));
            try
            {
                if (request.Activate)
                    sector.Activate();
                else
                    sector.Deactivate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("OrderSector.StatusError", ex.Message));
            }
            _orderSectorRepository.Update(sector);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Status do setor {SectorId} alterado. Ativo: {IsActive}", request.Id, request.Activate);
            return Result.Success();
        }
    }
}