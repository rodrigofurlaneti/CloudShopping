using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Security;
public sealed class AuthSession : Entity<string>
{
    public int TenantId { get; private set; }
    public int SubjectId { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public string CredentialStamp { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    private AuthSession() { }
    public static AuthSession Create(int tenantId,int subjectId,string kind,string stamp,DateTime expiresAt,string? id=null)
    {
        if(tenantId<=0||subjectId<=0||kind is not ("Customer" or "Administrator")) throw new ArgumentException("Identidade de sessão inválida.");
        return new AuthSession {Id=id??Guid.NewGuid().ToString("N"),TenantId=tenantId,SubjectId=subjectId,Kind=kind,CredentialStamp=stamp,ExpiresAt=expiresAt};
    }
    public void Revoke(DateTime? revokedAt=null) => RevokedAt ??= revokedAt??DateTime.UtcNow;
}
