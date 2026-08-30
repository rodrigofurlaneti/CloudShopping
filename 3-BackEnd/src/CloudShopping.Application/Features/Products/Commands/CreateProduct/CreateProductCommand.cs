using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Products.Commands.CreateProduct
{
    public sealed record CreateProductCommand(
        string Sku,
        string Name,
        decimal Price,
        int InitialStock = 0,
        string? Aisle = null,
        string? Rack = null,
        string? Level = null,
        string? Position = null) : IRequest<Result<int>>;
}
