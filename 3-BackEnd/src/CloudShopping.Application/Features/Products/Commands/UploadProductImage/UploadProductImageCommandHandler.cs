using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.UploadProductImage
{
    public sealed class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, Result<string>>
    {
        private readonly IProductRepository _productRepository;
        // Supondo que você crie um IProductImageRepository ou adicione ao repositório principal
        private readonly IProductImageRepository _imageRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UploadProductImageCommandHandler> _logger;

        public UploadProductImageCommandHandler(
            IProductRepository productRepository,
            IProductImageRepository imageRepository,
            IFileStorageService fileStorageService,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<UploadProductImageCommandHandler> logger)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _fileStorageService = fileStorageService;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null || product.TenantId != tenantId)
            {
                return Result.Failure<string>(new Error("Product.NotFound", "Produto não encontrado ou não autorizado."));
            }

            try
            {
                var relativePath = await _fileStorageService.SaveProductImageAsync(tenantId, product.Id, request.File, cancellationToken);
                var productImage = ProductImage.Create(
                    productId: product.Id,
                    fileName: request.File.FileName,
                    filePath: relativePath,
                    isPrimary: request.IsPrimary,
                    displayOrder: request.DisplayOrder
                );
                await _imageRepository.AddAsync(productImage, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Imagem enviada com sucesso para o produto {ProductId}. Caminho: {Path}", product.Id, relativePath);
                return Result.Success(relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload de imagem para o produto {ProductId}.", request.ProductId);
                return Result.Failure<string>(new Error("Product.ImageUploadFailed", ex.Message));
            }
        }
    }
}
