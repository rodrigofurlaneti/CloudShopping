using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Storefront.Contracts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using MediatR;
using System.Text.Json;
namespace CloudShopping.Application.Features.Storefront.Queries.GetStoreProduct;
public sealed class GetStoreProductQueryHandler(IStorefrontRepository repository) : IRequestHandler<GetStoreProductQuery, Result<StoreProductDetails?>>
{
    public Task<Result<StoreProductDetails?>> Handle(GetStoreProductQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<StoreProductDetails?> ExecuteAsync(GetStoreProductQuery request, CancellationToken ct)
    {
        var p = await repository.Product(request.Id, request.Slug, ct);
        if (p == null) return null;
        return new(p.Id, p.Name, p.Sku, p.Slug, p.Price, p.DepartmentId, p.Description, p.Brand, p.WeightKg, p.WidthCm, p.HeightCm, p.LengthCm,
            p.FamilyCode, p.VariantLabel, (string.IsNullOrWhiteSpace(p.AttributesJson) ? new Dictionary<string,string>() : JsonSerializer.Deserialize<Dictionary<string,string>>(p.AttributesJson) ?? new()), await repository.Variants(p.FamilyCode, ct),
            p.AvailableStock, p.Images.Where(x => x.IsActive).OrderByDescending(x => x.IsPrimary).ThenBy(x => x.DisplayOrder).Select(x => x.FilePath).ToArray());
    }
}

