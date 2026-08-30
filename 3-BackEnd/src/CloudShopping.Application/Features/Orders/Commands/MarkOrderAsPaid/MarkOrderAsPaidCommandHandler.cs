using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsPaid
{
    public sealed class MarkOrderAsPaidCommandHandler : IRequestHandler<MarkOrderAsPaidCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkOrderAsPaidCommandHandler> _logger;

        public MarkOrderAsPaidCommandHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<MarkOrderAsPaidCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de pagamento em pedido inexistente. OrderId: {OrderId}", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Acesso negado. OrderId: {OrderId}, Lojista: {TenantId}", request.OrderId, tenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence à sua loja."));
            }
            var amount = request.Amount >= 0 ? request.Amount : order.TotalAmount;
            try
            {
                order.AddApprovedPayment(request.PaymentMethod, amount);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao marcar pedido {OrderId} como pago.", request.OrderId);
                return Result.Failure(new Error("Order.PaymentFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pedido {OrderId} marcado como pago. Valor: {Amount}, Método: {PaymentMethod}",
                request.OrderId, amount, request.PaymentMethod);
            return Result.Success();
        }
    }
}