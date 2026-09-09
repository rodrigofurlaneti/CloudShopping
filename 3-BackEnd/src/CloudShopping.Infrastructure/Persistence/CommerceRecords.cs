namespace CloudShopping.Infrastructure.Persistence;

public sealed class AuthSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int TenantId { get; set; }
    public int SubjectId { get; set; }
    public string Kind { get; set; } = "";
    public string CredentialStamp { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
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

