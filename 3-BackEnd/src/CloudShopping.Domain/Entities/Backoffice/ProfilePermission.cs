using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Backoffice;

public sealed class ProfilePermission : Entity<int>
{
    public int TenantId { get; private set; }
    public int ProfileId { get; private set; }
    public string Permission { get; private set; } = string.Empty;

    private ProfilePermission() { }

    public static ProfilePermission Create(int tenantId, int profileId, string permission)
    {
        if (tenantId <= 0 || profileId <= 0) throw new ArgumentException("Empresa e perfil são obrigatórios.");
        if (!PermissionPolicy.Available.Contains(permission)) throw new ArgumentException("Permissão inválida.");
        return new ProfilePermission { TenantId = tenantId, ProfileId = profileId, Permission = permission };
    }
}
