using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // Para ITenantProvider
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed
{
    public sealed class MarkDeliveryFailedCommandHandler : IRequestHandler<MarkDeliveryFailedCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkDeliveryFailedCommandHandler> _logger;

        public MarkDeliveryFailedCommandHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<MarkDeliveryFailedCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(MarkDeliveryFailedCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de registrar falha de entrega num pedido inexistente. OrderId: {OrderId}", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Tentativa não autorizada. OrderId: {OrderId}, Tenant Autenticado: {TenantId}, Tenant Pedido: {OrderTenantId}",
                    request.OrderId, tenantId, order.TenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence à sua loja."));
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