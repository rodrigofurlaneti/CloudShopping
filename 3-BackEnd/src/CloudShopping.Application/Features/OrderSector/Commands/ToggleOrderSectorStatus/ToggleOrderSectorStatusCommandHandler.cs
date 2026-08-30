using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderSector.Commands.ToggleOrderSectorStatus
{
    public sealed class ToggleOrderSectorStatusCommandHandler : IRequestHandler<ToggleOrderSectorStatusCommand, Result>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleOrderSectorStatusCommandHandler(IOrderSectorRepository orderSectorRepository, IUnitOfWork unitOfWork)
        {
            _orderSectorRepository = orderSectorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ToggleOrderSectorStatusCommand request, CancellationToken cancellationToken)
        {
            var sector = await _orderSectorRepository.GetByIdAsync(request.Id, cancellationToken);
            if (sector is null)
                return Result.Failure(new Error("OrderSector.NotFound", "Setor de pedido não encontrado."));

            try
            {
                if (request.Activate) sector.Activate();
                else sector.Deactivate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("OrderSector.InvalidOperation", ex.Message));
            }

            _orderSectorRepository.Update(sector);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
