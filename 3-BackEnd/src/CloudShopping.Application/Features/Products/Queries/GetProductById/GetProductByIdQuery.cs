using CloudShopping.Application.Features.Products.ViewModels;
using MediatR;

namespace CloudShopping.Application.Features.Products.Queries.GetProductById
{
    public sealed record GetProductByIdQuery(int Id) : IRequest<ProductViewModel?>;
}
