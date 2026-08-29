using MediatR;

namespace CloudShopping.Application.Features.Orders.Events
{
    public sealed class OrderCanceledDomainEventHandler : INotificationHandler<OrderCanceledDomainEvent>
    {
        private readonly IOrderHistoryRepository _historyRepository;
        private readonly IProductRepository _productRepository;

        public OrderCanceledDomainEventHandler(
            IOrderHistoryRepository historyRepository, 
            IProductRepository productRepository)
        {
            _historyRepository = historyRepository;
            _productRepository = productRepository;
        }

        public async Task Handle(OrderCanceledDomainEvent notification, CancellationToken cancellationToken)
        {
            var historyRecord = new OrderStateHistory(
                notification.OrderId, 
                OrderStatus.Canceled, 
                "Pedido cancelado pelo usuário."
            );
            await _historyRepository.AddAsync(historyRecord, cancellationToken);

            // 2. Aciona lógica para reverter a reserva de estoque (ReservedStock -> PhysicalStock)
            // await _productRepository.RevertReservedStockAsync(notification.OrderId, cancellationToken);
        }
    }
}