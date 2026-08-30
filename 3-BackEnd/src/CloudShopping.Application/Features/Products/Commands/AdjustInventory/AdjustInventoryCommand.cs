using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Products.Commands.AdjustInventory
{
    public sealed record AdjustInventoryCommand(
            int ProductId,
            int NewPhysicalQuantity,
            string Reason
        ) : IRequest<Result>;
}
