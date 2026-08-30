using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelOrderCommandHandler> _logger;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<CancelOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de cancelamento falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Cancelamento não autorizado. OrderId: {OrderId}, Lojista Esperado: {TenantId}, Lojista Real: {OrderTenantId}",
                    request.OrderId, tenantId, order.TenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Você não tem permissão para cancelar este pedido."));
            }
            try
            {
                order.CancelOrder();
                _orderRepository.Update(order);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Pedido {OrderId} cancelado com sucesso (Tenant: {TenantId}).",
                    request.OrderId, tenantId);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Falha na regra de negócio ao cancelar o Pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.CancellationError", ex.Message));
            }
        }
    }
}