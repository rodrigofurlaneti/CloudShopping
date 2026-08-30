using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.AdjustInventory
{
    public sealed class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, Result>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdjustInventoryCommandHandler> _logger;

        public AdjustInventoryCommandHandler(
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<AdjustInventoryCommandHandler> logger)
        {
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (product is null || product.TenantId != tenantId)
                return Result.Failure(new Error("Product.NotFound", "Produto não encontrado ou não autorizado."));

            var previousStock = product.PhysicalStock;
            var difference = request.NewPhysicalQuantity - previousStock;

            try
            {
                // Executa a regra de domínio (valida se o novo estoque não é menor que as reservas ativas)
                product.AdjustInventory(request.NewPhysicalQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao ajustar inventário do produto {ProductId} para {NewQty}.", request.ProductId, request.NewPhysicalQuantity);
                return Result.Failure(new Error("Product.AdjustmentFailed", ex.Message));
            }

            // Registra a divergência na auditoria caso tenha havido alteração real
            if (difference != 0)
            {
                var movement = StockMovement.Create(
                    productId: product.Id,
                    movementType: StockMovementType.Adjustment,
                    quantityChanged: difference,
                    balanceAfter: product.PhysicalStock,
                    reason: request.Reason
                );
                await _stockMovementRepository.AddAsync(movement, cancellationToken);
            }

            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Inventário do produto {ProductId} ajustado de {Old} para {New}.", request.ProductId, previousStock, product.PhysicalStock);
            return Result.Success();
        }
    }
}
