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
            return await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductImage?> GetPrimaryByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.IsPrimary)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(ProductImage productImage, CancellationToken cancellationToken = default)
        {
            await _context.ProductImages.AddAsync(productImage, cancellationToken);
        }

        public void Update(ProductImage productImage)
        {
            _context.ProductImages.Update(productImage);
        }

        public void Remove(ProductImage productImage)
        {
            _context.ProductImages.Remove(productImage);
        }
    }
}
