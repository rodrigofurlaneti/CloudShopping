using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderStateHistories.Commands.DeactivateOrderHistory
{
    public sealed class DeactivateOrderHistoryCommandHandler : IRequestHandler<DeactivateOrderHistoryCommand, Result>
    {
        private readonly IOrderStateHistoryRepository _historyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateOrderHistoryCommandHandler> _logger;

        public DeactivateOrderHistoryCommandHandler(
            IOrderStateHistoryRepository historyRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateOrderHistoryCommandHandler> logger)
        {
            _historyRepository = historyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeactivateOrderHistoryCommand request, CancellationToken cancellationToken)
        {
            var historyRecord = await _historyRepository.GetByIdAsync(request.HistoryId, cancellationToken);
            if (historyRecord is null)
                return Result.Failure(new Error("OrderHistory.NotFound", "Registro de histórico não encontrado."));

            try
            {
                historyRecord.Deactivate();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("OrderHistory.StatusError", ex.Message));
            }
            _historyRepository.Update(historyRecord);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogWarning("Registro de histórico {HistoryId} foi inativado (Soft Delete) pelo Administrador.", request.HistoryId);
            return Result.Success();
        }
    }
}
