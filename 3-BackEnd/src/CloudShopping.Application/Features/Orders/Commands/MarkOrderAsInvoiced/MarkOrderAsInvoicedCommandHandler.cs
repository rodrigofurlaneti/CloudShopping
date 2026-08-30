using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced
{
    public sealed class MarkOrderAsInvoicedCommandHandler : IRequestHandler<MarkOrderAsInvoicedCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MarkOrderAsInvoicedCommandHandler> _logger;

        public MarkOrderAsInvoicedCommandHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<MarkOrderAsInvoicedCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(MarkOrderAsInvoicedCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de faturamento falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Tentativa não autorizada. OrderId: {OrderId}, Lojista Esperado: {TenantId}, Lojista Real: {OrderTenantId}",
                    request.OrderId, tenantId, order.TenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence à sua loja."));
            }
            try
            {
                order.MarkAsInvoiced(request.InvoiceKey);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de transição de status ao tentar faturar o pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pedido {OrderId} faturado com sucesso (Chave NFe: {InvoiceKey}).", request.OrderId, request.InvoiceKey);
            return Result.Success();
        }
    }
}