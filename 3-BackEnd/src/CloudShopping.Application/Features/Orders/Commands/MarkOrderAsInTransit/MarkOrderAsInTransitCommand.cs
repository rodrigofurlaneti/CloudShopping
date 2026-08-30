using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInTransit
{
    public sealed record MarkOrderAsInTransitCommand(int OrderId) : IRequest<Result>;

    public sealed class MarkOrderAsInTransitCommandHandler : IRequestHandler<MarkOrderAsInTransitCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkOrderAsInTransitCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(MarkOrderAsInTransitCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));

            try
            {
                order.MarkAsIntransit();
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
