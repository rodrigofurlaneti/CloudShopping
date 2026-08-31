using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.OrderState.Commands.ToggleOrderStatusStatus
{
    // Handler adicionado seguindo o mesmo padrão do ToggleOrderSectorStatusCommandHandler:
    // não existia endpoint algum para ativar/desativar um status de pedido, apenas os
    // métodos Activate()/Deactivate() já existentes na entidade de domínio.
    public sealed class ToggleOrderStatusStatusCommandHandler : IRequestHandler<ToggleOrderStatusStatusCommand, Result>
    {
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleOrderStatusStatusCommandHandler(IOrderStatusRepository orderStatusRepository, IUnitOfWork unitOfWork)
        {
            _orderStatusRepository = orderStatusRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ToggleOrderStatusStatusCommand request, CancellationToken cancellationToken)
        {
            var status = await _orderStatusRepository.GetByIdAsync(request.Id, cancellationToken);
            if (status is null)
                return Result.Failure(new Error("OrderStatus.NotFound", "Status de pedido não encontrado."));

            try
            {
                if (request.Activate) status.Activate();
                else status.Deactivate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("OrderStatus.InvalidOperation", ex.Message));
            }

            _orderStatusRepository.Update(status);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
