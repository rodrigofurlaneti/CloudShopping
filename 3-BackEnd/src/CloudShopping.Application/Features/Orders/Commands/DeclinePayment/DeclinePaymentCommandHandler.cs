using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.DeclinePayment
{
    public sealed class DeclinePaymentCommandHandler : IRequestHandler<DeclinePaymentCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeclinePaymentCommandHandler> _logger;

        public DeclinePaymentCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeclinePaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeclinePaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de recusar pagamento para um pedido inexistente. OrderId: {OrderId}", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            try
            {
                order.UpdatePaymentDeclined(request.PaymentId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação de negócio ao recusar pagamento {PaymentId} do pedido {OrderId}.", request.PaymentId, request.OrderId);
                return Result.Failure(new Error("Payment.CannotDecline", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pagamento {PaymentId} recusado com sucesso para o pedido {OrderId}.", request.PaymentId, request.OrderId);
            return Result.Success();
        }
    }
}