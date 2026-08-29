using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed
{
    public sealed class MarkDeliveryFailedCommandHandler : IRequestHandler<MarkDeliveryFailedCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkDeliveryFailedCommandHandler> _logger;
        public MarkDeliveryFailedCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<MarkDeliveryFailedCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(MarkDeliveryFailedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null || order.TenantId != request.TenantId)
            {
                _logger.LogWarning("Tentativa inválida de registrar falha de entrega. OrderId: {OrderId}, TenantId: {TenantId}", request.OrderId, request.TenantId);
                return Result.Failure(new Error("Order.Invalid", "Pedido não encontrado ou não pertence a este lojista."));
            }
            try
            {
                order.MarkAsDeliveryFailed();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de transição de status ao tentar registrar falha na entrega para o pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Falha na entrega registrada para o pedido {OrderId}. Motivo: {Reason}", request.OrderId, request.Reason);
            return Result.Success();
        }
    }
}
