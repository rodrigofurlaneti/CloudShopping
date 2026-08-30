using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Domain.Entities.Products; // Para o StockMovement
using CloudShopping.Domain.Enums; // Para o StockMovementType
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.ShipOrder
{
    public sealed class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository; // Adicionado para auditoria
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShipOrderCommandHandler> _logger;

        public ShipOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<ShipOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Tentativa de despachar pedido inexistente. OrderId: {OrderId}", request.OrderId);
                return Result.Failure(new Error("Order.NotFound", "Pedido não encontrado."));
            }
            if (order.TenantId != tenantId)
            {
                _logger.LogWarning("Tentativa não autorizada. OrderId: {OrderId}, Lojista: {TenantId}", request.OrderId, tenantId);
                return Result.Failure(new Error("Order.Unauthorized", "Este pedido não pertence à sua loja."));
            }
            try
            {
                order.ShipOrder();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro ao tentar despachar o pedido {OrderId}.", request.OrderId);
                return Result.Failure(new Error("Order.TransitionFailed", ex.Message));
            }
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.CommitReservedStock(item.Quantity);
                    _productRepository.Update(product);
                    var movement = StockMovement.Create(
                        productId: product.Id,
                        movementType: StockMovementType.Sale,
                        quantityChanged: -item.Quantity,
                        balanceAfter: product.PhysicalStock,
                        reason: $"Expedição do Pedido #{order.Id}"
                    );
                    await _stockMovementRepository.AddAsync(movement, cancellationToken);
                }
            }
            _orderRepository.Update(order);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Pedido {OrderId} despachado com sucesso. Estoques atualizados.", request.OrderId);
            return Result.Success();
        }
    }
}