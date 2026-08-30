using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.UpdateProductDetails
{
    public sealed class UpdateProductDetailsCommandHandler : IRequestHandler<UpdateProductDetailsCommand, Result>
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProductDetailsCommandHandler> _logger;

        public UpdateProductDetailsCommandHandler(
            IProductRepository productRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<UpdateProductDetailsCommandHandler> logger)
        {
            _productRepository = productRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

            if (product is null)
            {
                _logger.LogWarning("Tentativa de atualizar produto inexistente. ProductId: {ProductId}", request.ProductId);
                return Result.Failure(new Error("Product.NotFound", "Produto não encontrado."));
            }

            if (product.TenantId != tenantId)
            {
                _logger.LogWarning("Tentativa não autorizada de atualizar produto. ProductId: {ProductId}, Tenant: {TenantId}", request.ProductId, tenantId);
                return Result.Failure(new Error("Product.Unauthorized", "Este produto não pertence à sua loja."));
            }

            try
            {
                product.UpdateDetails(request.Name, request.Price);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar detalhes do produto {ProductId}.", request.ProductId);
                return Result.Failure(new Error("Product.InvalidData", ex.Message));
            }

            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Detalhes do produto {ProductId} atualizados com sucesso.", request.ProductId);
            return Result.Success();
        }
    }
}
