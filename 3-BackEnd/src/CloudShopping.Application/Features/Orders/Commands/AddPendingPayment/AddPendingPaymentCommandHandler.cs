using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.AddPendingPayment
{
    public sealed class AddPendingPaymentCommandHandler : IRequestHandler<AddPendingPaymentCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddPendingPaymentCommandHandler> _logger;

        public AddPendingPaymentCommandHandler(
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<AddPendingPaymentCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result> Handle(AddPendingPaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de adicionar pagamento falhou. Pedido {OrderId} não encontrado.", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            try
            {
                order.AddPendingPayment(request.PaymentMethod, request.Amount);
                _orderRepository.Update(order);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Pagamento pendente de {Amount} adicionado com sucesso ao Pedido {OrderId} via {PaymentMethod}.",
                    request.Amount, request.OrderId, request.PaymentMethod);
                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Violação de regra de negócio ao adicionar pagamento no Pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.PaymentError", ex.Message));
            }
        }
    }
}