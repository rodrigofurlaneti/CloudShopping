using CloudShopping.Application.Abstractions.Data;
namespace CloudShopping.Application.Features.Storefront.Contracts;
public sealed record StoreProductDetails(int Id, string Name, string Sku, string Slug, decimal Price, int DepartmentId, string Description,
    string? Brand, decimal WeightKg, decimal WidthCm, decimal HeightCm, decimal LengthCm, string? FamilyCode, string? VariantLabel,
    Dictionary<string,string> Attributes, IReadOnlyList<StoreVariantView> Variants, int AvailableStock, string[] Images);
