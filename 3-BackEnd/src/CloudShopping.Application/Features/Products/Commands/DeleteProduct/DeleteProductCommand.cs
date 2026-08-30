using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Products.Commands.DeleteProduct
{
    public sealed record DeleteProductCommand(int Id) : IRequest<Result>;
}
