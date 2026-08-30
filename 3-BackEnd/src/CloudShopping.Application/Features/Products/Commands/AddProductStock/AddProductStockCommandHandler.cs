using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products.CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.AddProductStock
{
    public sealed class AddProductStockCommandHandler : IRequestHandler<AddProductStockCommand, Result>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddProductStockCommandHandler> _logger;

        public AddProductStockCommandHandler(
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<AddProductStockCommandHandler> logger)
        {
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(AddProductStockCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null || product.TenantId != tenantId)
                return Result.Failure(new Error("Product.NotFound", "Produto não encontrado ou não autorizado."));

            try
            {
                product.AddPhysicalStock(request.Quantity);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tentativa inválida de adicionar estoque para o produto {ProductId}.", request.ProductId);
                return Result.Failure(new Error("Product.InvalidStock", ex.Message));
            }
            var movement = StockMovement.Create(
                productId: product.Id,
                movementType: StockMovementType.Purchase,
                quantityChanged: request.Quantity,
                balanceAfter: product.PhysicalStock,
                reason: request.Reason
            );
            await _stockMovementRepository.AddAsync(movement, cancellationToken);
            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Adicionadas {Quantity} unidades ao produto {ProductId}. Novo saldo físico: {Stock}",
                request.Quantity, product.Id, product.PhysicalStock);
            return Result.Success();
        }
    }
}
