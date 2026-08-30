using CloudShopping.Application.Abstractions.Data;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
    {
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IOrderStatusRepository orderStatusRepository, IUnitOfWork unitOfWork)
        {
            _orderStatusRepository = orderStatusRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var status = await _orderStatusRepository.GetByIdAsync(request.Id, cancellationToken);
            if (status is null)
                throw new KeyNotFoundException($"Status de pedido {request.Id} não encontrado.");

            status.Update(request.OrderSectorId, request.Name);

            _orderStatusRepository.Update(status);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
