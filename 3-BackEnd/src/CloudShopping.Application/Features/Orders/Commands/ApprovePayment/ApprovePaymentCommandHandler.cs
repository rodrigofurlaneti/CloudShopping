using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.ApprovePayment
{
    public sealed class ApprovePaymentCommandHandler : IRequestHandler<ApprovePaymentCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ApprovePaymentCommandHandler> _logger;
        public ApprovePaymentCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<ApprovePaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(ApprovePaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de aprovar pagamento falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            try
            {
                order.UpdatePaymentApproved(request.PaymentId);
                _orderRepository.Update(order);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Pagamento {PaymentId} aprovado com sucesso no Pedido {OrderId}.",
                    request.PaymentId, request.OrderId);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Falha na regra de negócio ao aprovar pagamento {PaymentId} no Pedido {OrderId}.",
                    request.PaymentId, request.OrderId);
                return Result.Failure(new Error("Order.PaymentApprovalError", ex.Message));
            }
        }
    }
}