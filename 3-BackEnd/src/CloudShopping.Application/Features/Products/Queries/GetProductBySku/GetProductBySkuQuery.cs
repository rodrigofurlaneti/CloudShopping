using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;

namespace CloudShopping.Application.Features.Products.Queries.GetProductBySku
{
    public sealed record GetProductBySkuQuery(string Sku) : IRequest<ProductViewModel?>;
}
