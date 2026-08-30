using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderStateHistories.Commands.DeactivateOrderHistory
{
    public sealed class DeactivateOrderHistoryCommandHandler : IRequestHandler<DeactivateOrderHistoryCommand, Result>
    {
        private readonly IOrderStateHistoryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateOrderHistoryCommandHandler(IOrderStateHistoryRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeactivateOrderHistoryCommand request, CancellationToken cancellationToken)
        {
            var history = await _repository.GetByIdAsync(request.HistoryId, cancellationToken);
            if (history is null)
                return Result.Failure(new Error("OrderStateHistory.NotFound", "Registro de histórico não encontrado."));

            try
            {
                history.Deactivate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("OrderStateHistory.InvalidOperation", ex.Message));
            }

            _repository.Update(history);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
