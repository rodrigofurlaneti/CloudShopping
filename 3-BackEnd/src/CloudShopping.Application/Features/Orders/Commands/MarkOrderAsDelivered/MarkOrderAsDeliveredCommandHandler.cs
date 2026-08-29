using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsDelivered
{
    public sealed class MarkOrderAsDeliveredCommandHandler : IRequestHandler<MarkOrderAsDeliveredCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkOrderAsDeliveredCommandHandler> _logger;

        public MarkOrderAsDeliveredCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<MarkOrderAsDeliveredCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(MarkOrderAsDeliveredCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null || order.TenantId != request.TenantId)
            {
                _logger.LogWarning("Tentativa inválida de marcar pedido como entregue. OrderId: {OrderId}, TenantId: {TenantId}", request.OrderId, request.TenantId);
                return Result.Failure(new Error("Order.Invalid", "Pedido não encontrado ou não pertence a este lojista."));
            }
            try
            {
                order.MarkAsDelivered();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de transição de status ao tentar marcar o pedido {OrderId} como entregue.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pedido {OrderId} marcado como entregue com sucesso.", request.OrderId);
            return Result.Success();
        }
    }
