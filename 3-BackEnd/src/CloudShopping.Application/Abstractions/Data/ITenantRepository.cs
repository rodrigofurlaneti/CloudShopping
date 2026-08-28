using CloudShopping.Domain.Entities.Tenants;
namespace CloudShopping.Application.Abstractions.Data
{
    public interface ITenantRepository : IRepository<Tenant, int> { }
}