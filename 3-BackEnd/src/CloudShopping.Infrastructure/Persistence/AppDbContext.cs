using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Entities.Store;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Domain.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        private readonly int _currentTenantId;
        private readonly IPublisher? _publisher;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider, IPublisher? publisher = null)
            : base(options)
        {
            _currentTenantId = tenantProvider.GetTenantId();
            _publisher = publisher;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Individual> Individuals => Set<Individual>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Contact> Contacts => Set<Contact>();

        // Nova tabela de Departamentos
        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<OrderStateHistory> OrderStateHistories => Set<OrderStateHistory>();
        public DbSet<OrderSector> OrderSectors => Set<OrderSector>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<StoreBanner> StoreBanners => Set<StoreBanner>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Isolamento multi-tenant + soft delete via filtros globais de consulta
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.IsActive && c.TenantId == _currentTenantId);
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.IsActive && o.TenantId == _currentTenantId);
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive && p.TenantId == _currentTenantId);

            // Filtro de Departamento: Retorna os do Tenant atual OU os globais do sistema (null)
            modelBuilder.Entity<Department>().HasQueryFilter(d => d.IsActive && (d.TenantId == _currentTenantId || d.TenantId == null));
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Coleta os eventos de domínio de todos os Aggregate Roots rastreados antes de persistir.
            var aggregateRoots = ChangeTracker.Entries()
                .Select(e => e.Entity)
                .OfType<IHasDomainEvents>()
                .Where(a => a.GetDomainEvents().Any())
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (_publisher is not null && aggregateRoots.Count > 0)
            {
                var domainEvents = aggregateRoots
                    .SelectMany(a => a.GetDomainEvents())
                    .ToList();

                foreach (var aggregate in aggregateRoots)
                {
                    aggregate.ClearDomainEvents();
                }

                foreach (var domainEvent in domainEvents)
                {
                    await _publisher.Publish(domainEvent, cancellationToken);
                }
            }

            return result;
        }
    }
}