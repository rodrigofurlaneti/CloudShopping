using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
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
                _logger.LogWarning("Tentativa de cancelar um pedido inexistente. OrderId: {OrderId}", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Cliente {CustomerId} tentou cancelar o pedido {OrderId} que pertence a outro usuário.", request.CustomerId, request.OrderId);
                return Result.Failure(new Error("Order.Unauthorized", "Você não tem permissão para cancelar este pedido."));
            }
            try
            {
                order.CancelOrder();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Regra de negócio violada ao cancelar pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.CannotCancel", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pedido {OrderId} cancelado com sucesso pelo cliente {CustomerId}.", request.OrderId, request.CustomerId);
            return Result.Success();
        }
    }
}