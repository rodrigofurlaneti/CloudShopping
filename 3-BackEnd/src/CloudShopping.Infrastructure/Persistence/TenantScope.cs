using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Entities.Store;
using CloudShopping.Domain.Entities.Tenants;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;

public sealed partial class AppDbContext
{
    // Only the gated onboarding transaction may provision a new tenant.
    public bool IsProvisioning { get; set; }
    public int CurrentTenantId => _currentTenantId;
    private void ConfigureTenantScope(ModelBuilder b)
    {
        b.Entity<Tenant>().HasQueryFilter(x => x.IsActive && x.Id == _currentTenantId);
        b.Entity<Employee>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<EmployeeUser>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<Profile>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<ProfileUser>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<StoreBanner>().HasQueryFilter(x => x.TenantId == null || x.TenantId == _currentTenantId);
        b.Entity<OrderStatus>().HasQueryFilter(x => x.TenantId == null || x.TenantId == _currentTenantId);
        b.Entity<OrderSector>().HasQueryFilter(x => x.TenantId == null || x.TenantId == _currentTenantId);
        b.Entity<Address>().HasQueryFilter(x => Customers.Any(c => c.Id == x.CustomerId));
        b.Entity<Individual>().HasQueryFilter(x => Customers.Any(c => c.Id == x.Id));
        b.Entity<Company>().HasQueryFilter(x => Customers.Any(c => c.Id == x.Id));
        b.Entity<Contact>().HasQueryFilter(x => Customers.Any(c => c.Id == x.CustomerId));
        b.Entity<Cart>().HasQueryFilter(x => Customers.Any(c => c.Id == x.CustomerId));
        b.Entity<CartItem>().HasQueryFilter(x => Carts.Any(c => c.Id == x.CartId));
        b.Entity<OrderItem>().HasQueryFilter(x => Orders.Any(o => o.Id == x.OrderId));
        b.Entity<OrderAddress>().HasQueryFilter(x => Orders.Any(o => o.Id == x.OrderId));
        b.Entity<Payment>().HasQueryFilter(x => Orders.Any(o => o.Id == x.OrderId));
        b.Entity<OrderStateHistory>().HasQueryFilter(x => Orders.Any(o => o.Id == x.OrderId));
        b.Entity<ProductImage>().HasQueryFilter(x => Products.Any(p => p.Id == x.ProductId));
        b.Entity<StockMovement>().HasQueryFilter(x => Products.IgnoreQueryFilters().Any(p => p.Id == x.ProductId && p.TenantId == _currentTenantId));
        b.Entity<AuthSession>().ToTable("authsessions").HasKey(x => x.Id);
        b.Entity<AuthSession>().Property(x => x.Id).HasColumnType("char(32)");
        b.Entity<ShippingOption>().ToTable("shippingoptions").HasKey(x => x.Id);
        b.Entity<ShippingOption>().Property(x => x.Amount).HasColumnType("decimal(12,2)");
        b.Entity<ShippingOption>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<OrderItem>().Property(x => x.ProductName).HasMaxLength(150);
        b.Entity<OrderItem>().Property(x => x.Sku).HasMaxLength(30);
    }

    private async Task GuardWrites(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var entries = ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
        foreach (var e in entries)
        {
            if (e.Entity is CloudShopping.Infrastructure.Services.AccessChange or CloudShopping.Infrastructure.Operations.OperationEvent or CloudShopping.Infrastructure.Operations.ReviewDecision or CloudShopping.Infrastructure.Operations.SupportMessage && e.State != EntityState.Added)
                throw new InvalidOperationException("Eventos operacionais são imutáveis; registre uma nova nota.");
            if (e.Metadata.FindProperty("Version") != null && e.State == EntityState.Modified)
                e.Property("Version").CurrentValue = checked((int)e.Property("Version").OriginalValue! + 1);
            if (IsProvisioning) continue;
            if (_currentTenantId <= 0) throw new UnauthorizedAccessException("Contexto da loja ausente.");
            if (e.Metadata.FindProperty("TenantId") != null)
            {
                if (e.Property("TenantId").CurrentValue is not int tenant || tenant != _currentTenantId ||
                    (e.State != EntityState.Added && !Equals(e.Property("TenantId").OriginalValue, tenant)))
                    throw new UnauthorizedAccessException("Recurso pertence a outro escopo.");
            }
            if (e.Entity is Tenant t && t.Id != _currentTenantId) throw new UnauthorizedAccessException();
            // Import rows are a staging area: invalid foreign IDs must remain reviewable, never applied.
            if (e.Entity is CloudShopping.Infrastructure.Operations.CatalogImportRow) continue;
            // Check foreign resources on writes as well as reads. Never trust caller-supplied IDs.
            foreach (var name in new[] { "CustomerId", "ProductId", "EmployeeId", "EmployeeUserId", "ProfileId", "DepartmentId", "OrderSectorId", "OrderStatusId" })
            {
                var prop = e.Metadata.FindProperty(name);
                if (prop == null || e.Property(name).CurrentValue is not int id || id <= 0) continue;
                bool owned = name switch {
                    "CustomerId" => await Customers.AnyAsync(x => x.Id == id, ct),
                    "ProductId" when e.Entity is StockMovement or CloudShopping.Infrastructure.Operations.ProductReview || e.Entity is CloudShopping.Infrastructure.Operations.WishlistItem && e.State==EntityState.Deleted => await Products.IgnoreQueryFilters().AnyAsync(x => x.Id == id && x.TenantId == _currentTenantId, ct),
                    "ProductId" => await Products.AnyAsync(x => x.Id == id, ct),
                    "EmployeeId" => await Set<Employee>().AnyAsync(x => x.Id == id, ct),
                    "EmployeeUserId" => await Set<EmployeeUser>().AnyAsync(x => x.Id == id, ct),
                    "ProfileId" => await Set<Profile>().AnyAsync(x => x.Id == id, ct),
                    "DepartmentId" => await Departments.AnyAsync(x => x.Id == id, ct),
                    "OrderSectorId" => await OrderSectors.AnyAsync(x => x.Id == id, ct),
                    "OrderStatusId" => await OrderStatuses.AnyAsync(x => x.Id == id, ct),
                    _ => true
                };
                if (!owned) throw new UnauthorizedAccessException("Relacionamento fora da loja.");
            }
        }
    }
}
