using CloudShopping.Domain.Entities.Carts;
using MediatR;
namespace CloudShopping.Application.Features.Carts.Queries.GetCartByCustomer
{
    public sealed record GetCartByCustomerQuery(int CustomerId) : IRequest<Cart?>;
}
