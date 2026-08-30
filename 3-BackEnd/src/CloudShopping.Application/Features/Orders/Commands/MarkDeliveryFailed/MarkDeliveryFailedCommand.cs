using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed
{
    public sealed record MarkDeliveryFailedCommand(int OrderId) : IRequest<Result>;

    public sealed class MarkDeliveryFailedCommandHandler : IRequestHandler<MarkDeliveryFailedCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkDeliveryFailedCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(MarkDeliveryFailedCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));

            try
            {
                order.MarkAsDeliveryFailed();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Order.InvalidOperation", ex.Message));
            }

            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
