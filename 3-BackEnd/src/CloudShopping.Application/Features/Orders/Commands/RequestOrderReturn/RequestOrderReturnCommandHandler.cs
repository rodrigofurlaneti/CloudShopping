using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn
{
    public sealed class RequestOrderReturnCommandHandler : IRequestHandler<RequestOrderReturnCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RequestOrderReturnCommandHandler> _logger;

        public RequestOrderReturnCommandHandler(
            IOrderRepository orderRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<RequestOrderReturnCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(RequestOrderReturnCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de devolução falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Devolução não autorizada. OrderId: {OrderId}, Lojista Esperado: {TenantId}, Lojista Real: {OrderTenantId}",
                    request.OrderId, tenantId, order.TenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence à sua loja."));
            }
            try
            {
                order.RequestReturn(request.Reason);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de transição de status ao tentar solicitar devolução para o pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Solicitação de devolução efetuada com sucesso (Pedido {OrderId}). Motivo: {Reason}", request.OrderId, request.Reason);
            return Result.Success();
        }
    }
}