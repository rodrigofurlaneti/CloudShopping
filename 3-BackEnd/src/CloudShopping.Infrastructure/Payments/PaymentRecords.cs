namespace CloudShopping.Infrastructure.Payments;

public sealed class AsaasConnection
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int TenantId { get; set; }
    public string Mode { get; set; } = "Direct";
    public string Environment { get; set; } = "Sandbox";
    public string WalletId { get; set; } = "";
    public string AccountKey { get; set; } = "";
    public string ProtectedApiKey { get; set; } = "";
    public string WebhookTokenHash { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public sealed class AsaasCustomer
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
    public string AccountKey { get; set; } = "";
    public string? RemoteId { get; set; }
    public bool RequestSent { get; set; }
}
public sealed class PaymentAttempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int TenantId { get; set; }
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string ConnectionId { get; set; } = "";
    public string AccountKey { get; set; } = "";
    public string Method { get; set; } = "";
    public decimal Amount { get; set; }
    public string SplitJson { get; set; } = "[]";
    public DateTime DueDate { get; set; }
    public string? RemoteCustomerId { get; set; }
    public string? RemotePaymentId { get; set; }
    public string? RemoteCheckoutId { get; set; }
    public string State { get; set; } = "Queued";
    public string ProviderStatus { get; set; } = "";
    public bool CreateSent { get; set; }
    public bool CreationRejected { get; set; }
    public bool CancelRequested { get; set; }
    public bool CancelSent { get; set; }
    public bool RefundRequested { get; set; }
    public bool RefundSent { get; set; }
    public bool RefundObserved { get; set; }
    public string? RefundRequestUrl { get; set; }
    public string? PaymentUrl { get; set; }
    public string? PixPayload { get; set; }
    public string? PixImage { get; set; }
    public string? BankSlipUrl { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime NextCheckAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}
public sealed class PaymentOperation
{
    public string Id {get;set;}=Guid.NewGuid().ToString("N");
    public int TenantId {get;set;}
    public int OrderId {get;set;}
    public string AttemptId {get;set;}="";
    public string Kind {get;set;}="";
    public string RequestedBy {get;set;}="";
    public DateTime RequestedAt {get;set;}=DateTime.UtcNow;
}
