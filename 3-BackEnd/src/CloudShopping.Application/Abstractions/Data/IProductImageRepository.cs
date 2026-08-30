using CloudShopping.Domain.Entities.Products;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IProductImageRepository : IRepository<ProductImage, int>
    {
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
        Task<ProductImage?> GetPrimaryByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    }
}