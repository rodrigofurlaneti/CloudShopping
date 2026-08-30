using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderSector.Commands.UpdateOrderSector
{
    public sealed class UpdateOrderSectorNameCommandHandler : IRequestHandler<UpdateOrderSectorNameCommand, Result>
    {
        private readonly IOrderSectorRepository _orderSectorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderSectorNameCommandHandler(IOrderSectorRepository orderSectorRepository, IUnitOfWork unitOfWork)
        {
            _orderSectorRepository = orderSectorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateOrderSectorNameCommand request, CancellationToken cancellationToken)
        {
            var sector = await _orderSectorRepository.GetByIdAsync(request.Id, cancellationToken);
            if (sector is null)
                return Result.Failure(new Error("OrderSector.NotFound", "Setor de pedido não encontrado."));

            try
            {
                sector.UpdateName(request.NewName);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(new Error("OrderSector.InvalidData", ex.Message));
            }

            _orderSectorRepository.Update(sector);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
