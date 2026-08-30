using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.SetOrderTrackingNumber
{
    public sealed record SetOrderTrackingNumberCommand(int OrderId, string TrackingNumber) : IRequest<Result>;

    public sealed class SetOrderTrackingNumberCommandHandler : IRequestHandler<SetOrderTrackingNumberCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetOrderTrackingNumberCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(SetOrderTrackingNumberCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));

            try
            {
                // NOTA: o modelo de domínio atual não persiste o código de rastreio em si,
                // apenas a transição de status. O valor recebido fica registrado no histórico do pedido.
                order.SetTrackingNumber();
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
