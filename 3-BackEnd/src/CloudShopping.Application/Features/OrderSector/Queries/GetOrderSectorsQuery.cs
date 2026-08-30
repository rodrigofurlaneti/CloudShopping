using CloudShopping.Application.Features.OrderSector.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.OrderSector.Queries
{
    public sealed record GetOrderSectorsQuery(bool OnlyActive = true) : IRequest<Result<IEnumerable<OrderSectorViewModel>>>;
}
