using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;

namespace CloudShopping.Application.Features.Products.Queries.GetProductBySku
{
    public sealed record GetProductBySkuQuery(string Sku) : IRequest<Result<ProductViewModel?>>;
}
