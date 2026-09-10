using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<Unit>>
    {
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IOrderStatusRepository orderStatusRepository, IUnitOfWork unitOfWork)
        {
            _orderStatusRepository = orderStatusRepository;
            _unitOfWork = unitOfWork;
        }

        public Task<Result<Unit>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
            => UseCaseExecution.Run(() => ExecuteAsync(request, cancellationToken), cancellationToken);

        private async Task<Unit> ExecuteAsync(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var status = await _orderStatusRepository.GetByIdAsync(request.Id, cancellationToken);
            if (status is null)
                throw new KeyNotFoundException($"Status de pedido {request.Id} não encontrado.");

            status.Update(request.OrderSectorId, request.Name);

            _orderStatusRepository.Update(status);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
