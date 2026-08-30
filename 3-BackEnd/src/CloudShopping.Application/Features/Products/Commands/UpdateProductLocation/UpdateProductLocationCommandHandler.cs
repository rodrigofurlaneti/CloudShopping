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

namespace CloudShopping.Application.Features.Products.Commands.UpdateProductLocation
{
    public sealed class UpdateProductLocationCommandHandler : IRequestHandler<UpdateProductLocationCommand, Result>
    {
        private readonly IProductRepository _productRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateProductLocationCommandHandler> _logger;

        public UpdateProductLocationCommandHandler(
            IProductRepository productRepository,
            ITenantProvider tenantProvider,
            IUnitOfWork unitOfWork,
            ILogger<UpdateProductLocationCommandHandler> logger)
        {
            _productRepository = productRepository;
            _tenantProvider = tenantProvider;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateProductLocationCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product is null || product.TenantId != tenantId)
                return Result.Failure(new Error("Product.NotFound", "Produto não encontrado ou não autorizado."));
            try
            {
                var location = StockLocation.Create(request.Aisle, request.Rack, request.Level, request.Position);
                product.UpdateLocation(location);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao atualizar localização do produto {ProductId}.", request.ProductId);
                return Result.Failure(new Error("Product.InvalidLocation", ex.Message));
            }
            _productRepository.Update(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Localização do produto {ProductId} atualizada com sucesso.", request.ProductId);
            return Result.Success();
        }
    }
}
