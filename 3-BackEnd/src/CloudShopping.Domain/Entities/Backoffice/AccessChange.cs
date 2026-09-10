using System.Text.Json;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Backoffice;

public sealed class AccessChange : Entity<string>
{
    public int TenantId { get; private set; }
    public int ProfileId { get; private set; }
    public int ActorId { get; private set; }
    public string BeforeJson { get; private set; } = "[]";
    public string AfterJson { get; private set; } = "[]";
    public DateTime CreatedAt { get; private set; }

    private AccessChange() { }

    public static AccessChange Create(int tenantId, int profileId, int actorId,
        IReadOnlyCollection<string> before, IReadOnlyCollection<string> after)
    {
        if (tenantId <= 0 || profileId <= 0 || actorId <= 0) throw new ArgumentException("Empresa, perfil e autor são obrigatórios.");
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        return new AccessChange
        {
            Id = Guid.NewGuid().ToString("N"), TenantId = tenantId, ProfileId = profileId, ActorId = actorId,
            BeforeJson = JsonSerializer.Serialize(before.Order().ToArray()),
            AfterJson = JsonSerializer.Serialize(after.Order().ToArray()), CreatedAt = DateTime.UtcNow
        };
    }
}
