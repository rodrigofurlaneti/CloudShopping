using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Products.Commands.UpdateProductLocation
{
    public sealed record UpdateProductLocationCommand(
            int ProductId,
            string Aisle,
            string Rack,
            string Level,
            string Position
        ) : IRequest<Result>;
}
