namespace CloudShopping.Infrastructure.Persistence;

public sealed class ShippingOption
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
    public string PostalCodePrefix { get; set; } = "";
    public int EstimatedDays { get; set; }
    public bool IsActive { get; set; } = true;
}

