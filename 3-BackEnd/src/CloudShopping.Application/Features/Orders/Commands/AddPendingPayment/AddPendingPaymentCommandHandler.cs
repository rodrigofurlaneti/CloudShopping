using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.AddPendingPayment
{
    public sealed class AddPendingPaymentCommandHandler : IRequestHandler<AddPendingPaymentCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddPendingPaymentCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(AddPendingPaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null) return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            order.AddPendingPayment(request.PaymentMethod, request.Amount);
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}
