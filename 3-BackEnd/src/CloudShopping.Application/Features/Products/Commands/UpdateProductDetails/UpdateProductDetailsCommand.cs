using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Products.Commands.UpdateProductDetails
{
    public sealed record UpdateProductDetailsCommand(
            int ProductId,
            string Name,
            decimal Price
        ) : IRequest<Result>;
}
