using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelOrderCommandHandler> _logger;

        public CancelOrderCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CancelOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de cancelamento falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Cancelamento não autorizado. Cliente {CustomerId} tentou cancelar o pedido {OrderId} pertencente ao cliente {OwnerId}.",
                    request.CustomerId, request.OrderId, order.CustomerId);
                return Result.Failure(new Error("Order.Unauthorized", "Você não tem permissão para cancelar este pedido."));
            }
            try
            {
                order.CancelOrder();
                _orderRepository.Update(order);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Pedido {OrderId} cancelado com sucesso pelo cliente {CustomerId}.",
                    request.OrderId, request.CustomerId);
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