using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<CreateProductCommandHandler> logger)
        {
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            StockLocation? location = null;
            if (!string.IsNullOrEmpty(request.Aisle) &&
                !string.IsNullOrEmpty(request.Rack) &&
                !string.IsNullOrEmpty(request.Level) &&
                !string.IsNullOrEmpty(request.Position))
            {
                location = StockLocation.Create(request.Aisle, request.Rack, request.Level, request.Position);
            }

            var existing = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
            if (existing is not null)
                return Result.Failure<int>(new Error("Product.DuplicateSku", $"Já existe um produto com o SKU '{request.Sku}'."));

            Product product;
            try
            {
                product = Product.Create(
                    tenantId: tenantId,
                    sku: request.Sku,
                    name: request.Name,
                    price: request.Price,
                    initialStock: request.InitialStock,
                    location: location
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar o produto SKU: {Sku} para o Tenant: {TenantId}", request.Sku, tenantId);
                return Result.Failure<int>(new Error("Product.InvalidData", ex.Message));
            }

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            if (request.InitialStock > 0)
            {
                var movement = StockMovement.Create(
                    productId: product.Id,
                    movementType: StockMovementType.PurchaseReceipt,
                    quantityChanged: request.InitialStock,
                    balanceAfter: request.InitialStock,
                    reason: "Estoque inicial cadastrado no sistema."
                );
                await _stockMovementRepository.AddAsync(movement, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }

            _logger.LogInformation("Produto criado com sucesso. ID: {ProductId}, SKU: {Sku}, Tenant: {TenantId}", product.Id, product.Sku, tenantId);
            return Result.Success(product.Id);
        }
    }
}
