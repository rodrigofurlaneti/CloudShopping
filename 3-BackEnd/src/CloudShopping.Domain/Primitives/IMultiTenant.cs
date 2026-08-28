namespace CloudShopping.Domain.Primitives
{
    public interface IMultiTenant
    {
        int TenantId { get; }
    }
}
