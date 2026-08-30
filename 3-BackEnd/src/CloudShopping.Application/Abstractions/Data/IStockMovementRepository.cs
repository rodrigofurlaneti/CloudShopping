using CloudShopping.Domain.Entities.Products;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IStockMovementRepository : IRepository<StockMovement, int>
    {
        Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    }
}
