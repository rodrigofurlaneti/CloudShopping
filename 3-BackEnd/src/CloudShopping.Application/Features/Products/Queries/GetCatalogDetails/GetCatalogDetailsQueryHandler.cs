using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using MediatR;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Products.ViewModels;
using CloudShopping.Application.Features.Products.Queries;
namespace CloudShopping.Application.Features.Products.Queries.GetCatalogDetails;
public sealed class GetCatalogDetailsQueryHandler(IProductRepository repository, ITenantProvider tenant) : IRequestHandler<GetCatalogDetailsQuery, Result<IReadOnlyList<CatalogDetailsView>>>
{
    public async Task<Result<IReadOnlyList<CatalogDetailsView>>> Handle(GetCatalogDetailsQuery request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (request.Page < 1 || request.Page > int.MaxValue / 20) return Result.Failure<IReadOnlyList<CatalogDetailsView>>(new Error("Catalog.InvalidPage", "Página inválida."));
        var (products, _) = await repository.GetPaginatedAsync(tenant.GetTenantId(), request.Page, 20, null, ct);
        var items = products.Select(x => new CatalogDetailsView(x.Id, x.Name, x.Sku, x.Version, x.Slug, x.Description, x.Brand,
            x.WeightKg, x.WidthCm, x.HeightCm, x.LengthCm, x.FamilyCode, x.VariantLabel, x.AttributesJson)).ToArray();
        return Result.Success<IReadOnlyList<CatalogDetailsView>>(items);
    }
}
