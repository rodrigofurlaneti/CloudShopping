using CloudShopping.Domain.Entities.Carts;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface ICartRepository : IRepository<Cart, int> { }
}
