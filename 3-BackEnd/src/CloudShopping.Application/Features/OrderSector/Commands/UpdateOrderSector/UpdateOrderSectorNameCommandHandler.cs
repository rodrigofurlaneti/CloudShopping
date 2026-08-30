using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
namespace CloudShopping.Application.Features.OrderSector.Commands.UpdateOrderSector
{
    public sealed class UpdateOrderSectorNameCommandHandler : IRequestHandler<UpdateOrderSectorNameCommand, Result>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderSectorNameCommandHandler> _logger;

        public UpdateOrderSectorNameCommandHandler(
            IOrderSectorRepository orderSectorRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderSectorNameCommandHandler> logger)
        {
            _orderSectorRepository = orderSectorRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateOrderSectorNameCommand request, CancellationToken cancellationToken)
        {
            var sector = await _orderSectorRepository.GetByIdAsync(request.Id, cancellationToken);
            if (sector is null) return Result.Failure(new Error("OrderSector.NotFound", "Setor não encontrado."));
            try
            {
                sector.UpdateName(request.NewName);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(new Error("OrderSector.InvalidName", ex.Message));
            }
            _orderSectorRepository.Update(sector);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Setor {SectorId} renomeado para {NewName}.", request.Id, request.NewName);
            return Result.Success();
        }
    }
}
