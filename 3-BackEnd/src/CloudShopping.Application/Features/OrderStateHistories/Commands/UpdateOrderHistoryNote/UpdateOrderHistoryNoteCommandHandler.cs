using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderStateHistories.Commands.UpdateOrderHistoryNote
{
    public sealed class UpdateOrderHistoryNoteCommandHandler : IRequestHandler<UpdateOrderHistoryNoteCommand, Result>
    {
        private readonly IOrderStateHistoryRepository _historyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderHistoryNoteCommandHandler> _logger;
        public UpdateOrderHistoryNoteCommandHandler(
            IOrderStateHistoryRepository historyRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderHistoryNoteCommandHandler> logger)
        {
            _historyRepository = historyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateOrderHistoryNoteCommand request, CancellationToken cancellationToken)
        {
            var historyRecord = await _historyRepository.GetByIdAsync(request.HistoryId, cancellationToken);
            if (historyRecord is null)
                return Result.Failure(new Error("OrderHistory.NotFound", "Registro de histórico não encontrado."));

            try
            {
                historyRecord.UpdateNotes(request.NewNote);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(new Error("OrderHistory.InvalidNote", ex.Message));
            }
            _historyRepository.Update(historyRecord);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Anotação do histórico {HistoryId} atualizada pelo Administrador.", request.HistoryId);
            return Result.Success();
        }
    }
}
