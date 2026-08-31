using System.Collections.Generic;

namespace CloudShopping.Application.Features.Products.ViewModels
{
    public sealed record ProductImageViewModel(int Id, string FileName, string FilePath, bool IsPrimary, int DisplayOrder);

    // DepartmentId/Aisle/Rack/Level/Position foram adicionados ao ViewModel:
    // o Product já expõe esses dados (DepartmentId e Location), mas o ViewModel
    // original não os incluía, o que impediria o painel de exibir/editar
    // departamento e endereçamento logístico do produto.
    public sealed record ProductViewModel(
        int Id,
        int DepartmentId,
        string Sku,
        string Name,
        decimal Price,
        int PhysicalStock,
        int ReservedStock,
        int AvailableStock,
        string? Aisle,
        string? Rack,
        string? Level,
        string? Position,
        IReadOnlyCollection<ProductImageViewModel> Images);

    public sealed record ProductSummaryViewModel(
        int Id,
        int DepartmentId,
        string Sku,
        string Name,
        decimal Price,
        int PhysicalStock,
        int ReservedStock,
        int AvailableStock,
        bool HasLocation,
        string? PrimaryImagePath);
}
