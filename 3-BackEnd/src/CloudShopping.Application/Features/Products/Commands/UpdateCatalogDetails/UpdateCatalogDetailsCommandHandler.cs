using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using System.Text.Json;
using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using MediatR;
using CloudShopping.Application.Features.Products.Commands;
namespace CloudShopping.Application.Features.Products.Commands.UpdateCatalogDetails;
public sealed class UpdateCatalogDetailsCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<UpdateCatalogDetailsCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(UpdateCatalogDetailsCommand request, CancellationToken ct)
        => CommandExecution.Run<Unit>(async () =>
    {
        var product = await repository.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("Produto não encontrado.");
        if (product.Version != request.Version) throw new InvalidOperationException("Produto atualizado. Recarregue a ficha antes de salvar.");
        if (request.Attributes == null || request.Attributes.Count > 12 || request.Attributes.Any(x => string.IsNullOrWhiteSpace(x.Key) || x.Key.Length > 80 || string.IsNullOrWhiteSpace(x.Value) || x.Value.Length > 150))
            throw new ArgumentException("Informe até 12 atributos, com nome e valor curtos.");
        var family = string.IsNullOrWhiteSpace(request.FamilyCode) ? null : request.FamilyCode.Trim();
        var variant = string.IsNullOrWhiteSpace(request.VariantLabel) ? null : request.VariantLabel.Trim();
        if (await repository.IsSlugOrVariantInUseAsync(request.Id, request.Slug, family, variant, ct)) throw new InvalidOperationException("URL ou variante já utilizada nesta loja.");
        product.ConfigureCatalog(request.Slug, request.Description, request.Brand, request.WeightKg, request.WidthCm, request.HeightCm, request.LengthCm, family, variant, JsonSerializer.Serialize(request.Attributes));
        await unitOfWork.CommitAsync(ct);

        // Invalida o cache Redis: Slug/Descrição/Marca/Dimensões/Atributos fazem parte
        // dos campos estáticos cacheados na ficha do produto (tarefa de cache full-stack).
        await cache.RemoveManyAsync(new[]
        {
            CacheKeys.TenantEntity(product.TenantId, CacheKeys.Products, product.Id),
            CacheKeys.ProductBySku(product.TenantId, product.Sku)
        }, ct);

        return Unit.Value;

    });
}
