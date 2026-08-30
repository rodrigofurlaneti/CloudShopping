using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _context;

        public ProductImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductImage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProductImage>()
                .FirstOrDefaultAsync(pi => pi.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ProductImage>()
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ProductImage productImage, CancellationToken cancellationToken = default)
        {
            await _context.Set<ProductImage>().AddAsync(productImage, cancellationToken);
        }

        public void Update(ProductImage productImage)
        {
            _context.Set<ProductImage>().Update(productImage);
        }

        public void Remove(ProductImage productImage)
        {
            _context.Set<ProductImage>().Remove(productImage);
        }
    }
}