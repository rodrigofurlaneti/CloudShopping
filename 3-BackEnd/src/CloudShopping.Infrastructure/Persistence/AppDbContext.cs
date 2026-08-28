using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        private readonly int _currentTenantId;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _currentTenantId = tenantProvider.GetTenantId();
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.IsActive && c.TenantId == _currentTenantId);
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.IsActive && o.TenantId == _currentTenantId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is IMultiTenant multiTenantEntity && multiTenantEntity.TenantId == 0)
                    {
                        typeof(IMultiTenant).GetProperty(nameof(IMultiTenant.TenantId))?.SetValue(multiTenantEntity, _currentTenantId);
                    }
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
