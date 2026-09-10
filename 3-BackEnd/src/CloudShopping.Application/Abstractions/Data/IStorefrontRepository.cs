using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Products;
namespace CloudShopping.Application.Abstractions.Data;

public sealed record StoreContextView(int Id, string CompanyName);
public sealed record StoreDepartmentView(int Id, string Name, string Slug);
public sealed record StoreProductSummary(int Id, string Name, string Sku, decimal Price, int DepartmentId, int AvailableStock, string? Image);
public sealed record StorePage<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
public sealed record StoreVariantView(int Id, string Slug, string? VariantLabel, decimal Price, int AvailableStock);
public sealed record StoreAddressView(int Id, string Street, string Number, string? Neighborhood, string City, string State, string ZipCode, bool IsDefault);
public sealed record StoreShippingView(int Id, string Name, decimal Amount, int EstimatedDays, string PostalCodePrefix, bool IsActive);
public sealed record StoreOrderSummary(int Id, decimal TotalAmount, int OrderStatusId, string ReservationState, DateTime? ReservationExpiresAt);
public interface IStorefrontRepository
{
    Task<StoreContextView> Context(CancellationToken ct);
    Task<IReadOnlyList<StoreDepartmentView>> Departments(CancellationToken ct);
    Task<StorePage<StoreProductSummary>> Products(string? search, int? departmentId, int page, int pageSize, CancellationToken ct);
    Task<Product?> Product(int? id, string? slug, CancellationToken ct);
    Task<IReadOnlyList<StoreVariantView>> Variants(string? familyCode, CancellationToken ct);
    Task<Customer?> Customer(int id, CancellationToken ct);
    Task<bool> EmailUsed(string email, int exceptCustomer, CancellationToken ct);
    Task<IReadOnlyList<StoreAddressView>> Addresses(int customerId, CancellationToken ct);
    Task<Address?> Address(int customerId, int addressId, CancellationToken ct);
    Task AddAddress(Address address, CancellationToken ct);
    Task<IReadOnlyList<StoreShippingView>> Shipping(string? zipCode, CancellationToken ct);
    Task<int> AddShipping(string name, decimal amount, int estimatedDays, string postalCodePrefix, CancellationToken ct);
    Task<bool> DisableShipping(int id, CancellationToken ct);
    Task<IReadOnlyList<StoreOrderSummary>> Orders(int customerId, int page, CancellationToken ct);
    Task Save(CancellationToken ct);
}
