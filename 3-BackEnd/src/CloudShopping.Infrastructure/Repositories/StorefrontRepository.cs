using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class StorefrontRepository(AppDbContext db) : IStorefrontRepository
{
    public Task<StoreContextView> Context(CancellationToken ct) => db.Tenants.Select(x => new StoreContextView(x.Id, x.CompanyName)).SingleAsync(ct);
    public async Task<IReadOnlyList<StoreDepartmentView>> Departments(CancellationToken ct) => await db.Departments.OrderBy(x => x.Name).Select(x => new StoreDepartmentView(x.Id, x.Name, x.Slug)).ToListAsync(ct);
    public async Task<StorePage<StoreProductSummary>> Products(string? search, int? departmentId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search) || x.Sku.Contains(search));
        if (departmentId != null) query = query.Where(x => x.DepartmentId == departmentId);
        var count = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new StoreProductSummary(x.Id, x.Name, x.Sku, x.Price, x.DepartmentId, x.PhysicalStock - x.ReservedStock,
                x.Images.Where(i => i.IsActive).OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.FilePath).FirstOrDefault())).ToListAsync(ct);
        return new(items, count, page, pageSize, (int)Math.Ceiling((double)count / pageSize));
    }
    public Task<Product?> Product(int? id, string? slug, CancellationToken ct) => db.Products.AsNoTracking().Include(x => x.Images).SingleOrDefaultAsync(x => id.HasValue ? x.Id == id : x.Slug == slug, ct);
    public async Task<IReadOnlyList<StoreVariantView>> Variants(string? familyCode, CancellationToken ct) => await db.Products.Where(x => familyCode != null && x.FamilyCode == familyCode).OrderBy(x => x.VariantLabel)
        .Select(x => new StoreVariantView(x.Id, x.Slug, x.VariantLabel, x.Price, x.PhysicalStock - x.ReservedStock)).ToListAsync(ct);
    public Task<Customer?> Customer(int id, CancellationToken ct) => db.Customers.Include(x => x.Individual).Include(x => x.Company).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> EmailUsed(string email, int exceptCustomer, CancellationToken ct) => db.Customers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == db.CurrentTenantId && x.Email == email && x.Id != exceptCustomer, ct);
    public async Task<IReadOnlyList<StoreAddressView>> Addresses(int customerId, CancellationToken ct) => await db.Addresses.Where(x => x.CustomerId == customerId && x.IsActive)
        .Select(x => new StoreAddressView(x.Id, x.Street, x.Number, x.Neighborhood, x.City, x.State, x.ZipCode, x.IsDefault)).ToListAsync(ct);
    public Task<Address?> Address(int customerId, int addressId, CancellationToken ct) => db.Addresses.SingleOrDefaultAsync(x => x.Id == addressId && x.CustomerId == customerId && x.IsActive, ct);
    public async Task AddAddress(Address address, CancellationToken ct) => await db.AddAsync(address, ct);
    public async Task<IReadOnlyList<StoreShippingView>> Shipping(string? zipCode, CancellationToken ct) => await db.Set<ShippingOption>()
        .Where(x => zipCode == null || x.IsActive && zipCode.StartsWith(x.PostalCodePrefix))
        .Select(x => new StoreShippingView(x.Id, x.Name, x.Amount, x.EstimatedDays, x.PostalCodePrefix, x.IsActive)).ToListAsync(ct);
    public async Task<int> AddShipping(string name, decimal amount, int estimatedDays, string postalCodePrefix, CancellationToken ct)
    {
        var option = new ShippingOption { TenantId = db.CurrentTenantId, Name = name, Amount = amount, EstimatedDays = estimatedDays, PostalCodePrefix = postalCodePrefix };
        db.Add(option); await db.SaveChangesAsync(ct); return option.Id;
    }
    public async Task<bool> DisableShipping(int id, CancellationToken ct)
    {
        var option = await db.Set<ShippingOption>().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (option == null) return false;
        option.IsActive = false; await db.SaveChangesAsync(ct); return true;
    }
    public async Task<IReadOnlyList<StoreOrderSummary>> Orders(int customerId, int page, CancellationToken ct) => await db.Orders.Where(x => x.CustomerId == customerId).OrderByDescending(x => x.Id)
        .Skip((page - 1) * 20).Take(20).Select(x => new StoreOrderSummary(x.Id, x.TotalAmount, x.OrderStatusId, x.ReservationState, x.ReservationExpiresAt)).ToListAsync(ct);
    public async Task Save(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
