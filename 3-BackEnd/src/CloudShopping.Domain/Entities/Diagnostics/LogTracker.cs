using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Diagnostics;

public sealed class LogTracker : Entity<long>
{
    private LogTracker() { }
    public int? TenantId { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long? AppUserId { get; private set; }
    public string? ActorType { get; private set; }
    public string? TraceId { get; private set; }
    public string? DirectoryName { get; private set; }
    public string ClassName { get; private set; } = "";
    public string MethodName { get; private set; } = "";
    public string Outcome { get; private set; } = "";
    public bool RequestAborted { get; private set; }
    public long? ExecutionTimeMs { get; private set; }
    public string? HttpMethod { get; private set; }
    public int? HttpStatusCode { get; private set; }
    public string? ExceptionType { get; private set; }
    public string? Message { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? StackTrace { get; private set; }
    public string? IpAddress { get; private set; }
    public static LogTracker Create(int? tenantId, LogTrackerData data)
    {
        if (tenantId is <= 0) throw new ArgumentException("Empresa inválida.");
        var entry = new LogTracker { TenantId = tenantId, CreatedAt = DateTime.UtcNow };
        entry.Apply(data);
        return entry;
    }
    public void UpdateDetails(LogTrackerData data, bool isActive)
    {
        Apply(data);
        IsActive = isActive;
        // Match MySQL datetime(6) precision.
        var now = DateTime.UtcNow;
        UpdatedAt = new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
    }
    private void Apply(LogTrackerData data)
    {
        Validate(data);
        AppUserId = data.AppUserId;
        ActorType = data.ActorType;
        TraceId = data.TraceId;
        DirectoryName = data.DirectoryName;
        ClassName = data.ClassName;
        MethodName = data.MethodName;
        Outcome = data.Outcome;
        RequestAborted = data.RequestAborted;
        ExecutionTimeMs = data.ExecutionTimeMs;
        HttpMethod = data.HttpMethod;
        HttpStatusCode = data.HttpStatusCode;
        ExceptionType = data.ExceptionType;
        Message = data.Message;
        ErrorMessage = data.ErrorMessage;
        StackTrace = data.StackTrace;
        IpAddress = data.IpAddress;
        IsSuccess = data.Outcome == "Success";
    }
    public static void Validate(LogTrackerData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Text(data.ClassName, 150, true); Text(data.MethodName, 150, true);
        Text(data.DirectoryName, 150); Text(data.TraceId, 128); Text(data.ExceptionType, 255);
        Text(data.HttpMethod, 10); Text(data.IpAddress, 45);
        foreach (var value in new[] { data.Message, data.ErrorMessage, data.StackTrace })
            if (value != null && System.Text.Encoding.UTF8.GetByteCount(value) > 65535)
                throw new ArgumentException("Texto de diagnóstico excede o limite da coluna TEXT.");
        if (data.Outcome is not ("Success" or "Error" or "Cancelled")) throw new ArgumentException("Resultado inválido.");
        if (data.ActorType is not (null or "Customer" or "Administrator" or "System")) throw new ArgumentException("Tipo de autor inválido.");
        if (data.AppUserId is <= 0 || data.AppUserId.HasValue && data.ActorType is not ("Customer" or "Administrator")) throw new ArgumentException("Autor inválido.");
        if (data.ExecutionTimeMs is < 0 || data.HttpStatusCode is < 100 or > 599) throw new ArgumentException("Duração ou status HTTP inválido.");
        if (data.RequestAborted && data.Outcome != "Cancelled") throw new ArgumentException("Requisição abortada deve ser classificada como cancelamento.");
    }
    private static void Text(string? value, int length, bool required = false)
    {
        if (required && string.IsNullOrWhiteSpace(value) || value?.Length > length)
            throw new ArgumentException("Campo de diagnóstico vazio ou acima do limite permitido.");
    }
}
