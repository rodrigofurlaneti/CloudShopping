using System.Collections.Generic;
using System.Linq;
using CloudShopping.Application.Features.Products.ViewModels;
using CloudShopping.Domain.Entities.Products;

namespace CloudShopping.Application.Features.Products.Caching
{
    // Snapshot cacheável de um produto SEM os campos de estoque dinâmico
    // (PhysicalStock/ReservedStock/AvailableStock) — conforme a tarefa de cache Redis,
    // que exige que o estoque nunca seja cacheado e sempre seja calculado em tempo real.
    // Usado por GetProductByIdQueryHandler e GetProductBySkuQueryHandler: o restante do
    // produto (nome, preço, endereçamento, imagens) vai para o Redis com TTL longo; o
    // estoque é buscado do MySQL a cada leitura (IProductRepository.GetStockAsync) e
    // mesclado aqui antes de devolver o ProductViewModel final ao chamador.
    public sealed record ProductStaticView(
        int Id,
        int DepartmentId,
        string Sku,
        string Name,
        decimal Price,
        string? Aisle,
        string? Rack,
        string? Level,
        string? Position,
        IReadOnlyCollection<ProductImageViewModel> Images)
    {
        public static ProductStaticView From(Product product) => new(
            product.Id,
            product.DepartmentId,
            product.Sku,
            product.Name,
            product.Price,
            product.Location?.Aisle,
            product.Location?.Rack,
            product.Location?.Level,
            product.Location?.Position,
            product.Images.Select(i => new ProductImageViewModel(i.Id, i.FileName, i.FilePath, i.IsPrimary, i.DisplayOrder)).ToList());

        public ProductViewModel ToViewModel(int physicalStock, int reservedStock) => new(
            Id,
            DepartmentId,
            Sku,
            Name,
            Price,
            physicalStock,
            reservedStock,
            physicalStock - reservedStock,
            Aisle,
            Rack,
            Level,
            Position,
            Images);
    }
}
