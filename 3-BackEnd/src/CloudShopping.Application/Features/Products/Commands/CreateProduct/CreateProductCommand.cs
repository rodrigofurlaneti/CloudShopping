using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
        int DepartmentId, 
        string Sku,
        string Name,
        decimal Price,
        int InitialStock,
        string? Aisle,
        string? Rack,
        string? Level,
        string? Position
    ) : IRequest<Result<int>>;
}