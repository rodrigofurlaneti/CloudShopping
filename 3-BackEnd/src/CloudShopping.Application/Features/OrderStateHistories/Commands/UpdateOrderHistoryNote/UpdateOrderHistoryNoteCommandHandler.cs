using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderStateHistories.Commands.UpdateOrderHistoryNote
{
    public sealed class UpdateOrderHistoryNoteCommandHandler : IRequestHandler<UpdateOrderHistoryNoteCommand, Result>
    {
        private readonly IOrderStateHistoryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderHistoryNoteCommandHandler(IOrderStateHistoryRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateOrderHistoryNoteCommand request, CancellationToken cancellationToken)
        {
            var history = await _repository.GetByIdAsync(request.HistoryId, cancellationToken);
            if (history is null)
                return Result.Failure(new Error("OrderStateHistory.NotFound", "Registro de histórico não encontrado."));

            try
            {
                history.UpdateNotes(request.NewNote);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(new Error("OrderStateHistory.InvalidData", ex.Message));
            }

            _repository.Update(history);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
