using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using System;
namespace CloudShopping.Infrastructure.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }
        public void Update(Product product)
        {
            _context.Products.Update(product);
        }
        public void Remove(Product product)
        {
            _context.Products.Remove(product);
        }
        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
        }
    }
}
