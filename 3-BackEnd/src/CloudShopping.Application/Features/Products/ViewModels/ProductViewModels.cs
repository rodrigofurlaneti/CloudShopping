using System.Collections.Generic;

namespace CloudShopping.Application.Features.Products.ViewModels
{
    public sealed record ProductImageViewModel(int Id, string FileName, string FilePath, bool IsPrimary, int DisplayOrder);

    public sealed record ProductViewModel(
        int Id,
        string Sku,
        string Name,
        decimal Price,
        int PhysicalStock,
        int ReservedStock,
        int AvailableStock,
        IReadOnlyCollection<ProductImageViewModel> Images);
}
