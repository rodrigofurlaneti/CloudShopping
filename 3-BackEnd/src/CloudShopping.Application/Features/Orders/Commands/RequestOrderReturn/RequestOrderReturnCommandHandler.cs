using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (order is null || order.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Tentativa inválida de solicitar devolução. OrderId: {OrderId}, CustomerId: {CustomerId}", request.OrderId, request.CustomerId);
                return Result.Failure(new Error("Order.Invalid", "Pedido não encontrado ou não pertence a este cliente."));
            }
            try
            {
                order.RequestReturn();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de transição de status ao tentar solicitar devolução para o pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Solicitação de devolução efetuada com sucesso para o pedido {OrderId}. Motivo: {Reason}", request.OrderId, request.Reason);
            return Result.Success();
        }
    }
}
