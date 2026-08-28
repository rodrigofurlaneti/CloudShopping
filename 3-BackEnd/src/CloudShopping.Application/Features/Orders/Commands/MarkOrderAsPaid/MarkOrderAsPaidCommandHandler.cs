using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsPaid
{
    public sealed class MarkOrderAsPaidCommandHandler : IRequestHandler<MarkOrderAsPaidCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        public MarkOrderAsPaidCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result> Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            try
            {
                order.MarkAsPaid();
                order.AddPayment(request.PaymentMethod, request.Amount, PaymentStatus.Approved);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(new Error("Order.InvalidState", ex.Message));
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
    }
}
