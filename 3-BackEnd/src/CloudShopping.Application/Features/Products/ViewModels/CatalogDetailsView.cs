namespace CloudShopping.Application.Features.Products.ViewModels;
public sealed record CatalogDetailsView(int Id, string Name, string Sku, int Version, string Slug, string Description, string? Brand,
    decimal WeightKg, decimal WidthCm, decimal HeightCm, decimal LengthCm, string? FamilyCode, string? VariantLabel, string AttributesJson);
