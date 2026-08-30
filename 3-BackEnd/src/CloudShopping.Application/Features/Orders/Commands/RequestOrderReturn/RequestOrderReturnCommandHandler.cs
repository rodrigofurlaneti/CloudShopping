using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn
{
    public sealed class RequestOrderReturnCommandHandler : IRequestHandler<RequestOrderReturnCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RequestOrderReturnCommandHandler> _logger;

        public RequestOrderReturnCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<RequestOrderReturnCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(RequestOrderReturnCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de devolução falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Devolução não autorizada. Cliente {CustomerId} tentou devolver o pedido {OrderId} do cliente {OwnerId}.",
                    request.CustomerId, request.OrderId, order.CustomerId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence a você."));
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