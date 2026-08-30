using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Events; // Namespace do seu evento

namespace CloudShopping.Application.Features.Orders.Events
{
    public sealed class OrderCanceledDomainEventHandler : INotificationHandler<OrderCanceledDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderCanceledDomainEventHandler> _logger;

        public OrderCanceledDomainEventHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ILogger<OrderCanceledDomainEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task Handle(OrderCanceledDomainEvent notification, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(notification.OrderId, cancellationToken);
            if (order == null) return;

            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);

                if (product != null)
                {
                    product.ReleaseReservedStock(item.Quantity);
                    _productRepository.Update(product);

                    _logger.LogInformation("Estoque liberado: {Quantity}x do Produto {ProductId} (Pedido Cancelado: {OrderId})",
                        item.Quantity, product.Id, order.Id);
                }
            }
        }
    }
}